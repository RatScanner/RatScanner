using RatScanner.FetchModels;
using RatScanner.FetchModels.TarkovTracker;
using RatStash;
using System;
using System.Linq;
using Item = RatStash.Item;

namespace RatScanner;

public static class ItemExtensions
{
	private static UserProgress? GetUserProgress()
	{
		UserProgress progress = null;
		if (RatConfig.Tracking.TarkovTracker.Enable && RatScannerMain.Instance.TarkovTrackerDB.Progress.Count >= 1)
		{
			var teamProgress = RatScannerMain.Instance.TarkovTrackerDB.Progress;
			progress = teamProgress.FirstOrDefault(x => x.UserId == RatScannerMain.Instance.TarkovTrackerDB.Self);
		}
		return progress;
	}

	public static int GetTaskRemaining(this Item item, UserProgress? progress = null)
	{
		progress ??= GetUserProgress();

		var count = 0;
		var showNonFir = RatConfig.Tracking.ShowNonFIRNeeds;
		var needed = TarkovDevAPI.GetNeededItems().tasks;
		var neededItems = needed.Where(i => i.Id == item.Id && (i.FoundInRaid || showNonFir));

		if (progress != null)
		{
			neededItems = neededItems.Where(i => !progress.Tasks.Any(h => h.Id == i.TaskId && h.Complete));
		}

		if (!neededItems.Any()) return 0;

		foreach (var neededItem in neededItems)
		{
			count += neededItem.Count;
			if (progress == null) continue;

			var taskProgress = progress.TaskObjectives.Where(part => part.Id == neededItem.ProgressId).ToList();
			foreach (var p in taskProgress)
			{
				if (p.Complete) continue;
				count -= p.Count;
			}
		}

		return count;
	}

	public static int GetHideoutRemaining(this Item item, UserProgress? progress = null)
	{
		progress ??= GetUserProgress();

		var count = 0;
		var needed = TarkovDevAPI.GetNeededItems().hideout;
		var neededItems = needed.Where(i => i.Id == item.Id);

		if (progress != null)
		{
			neededItems = neededItems.Where(i => !progress.HideoutModules.Any(h => h.Id == i.ModuleId && h.Complete));
		}

		if (!neededItems.Any()) return 0;

		foreach (var neededItem in neededItems)
		{
			count += neededItem.Count;
			if (progress == null) continue;

			var partProgress = progress.HideoutParts.Where(part => part.Id == neededItem.ProgressId).ToList();
			foreach (var p in partProgress)
			{
				if (p.Complete) continue;
				count -= p.Count;
			}
		}

		return count;
	}

	public static MarketItem GetMarketItem(this Item item)
	{
		var marketItem = RatScannerMain.Instance.MarketDB.GetItemById(item.Id);
		return marketItem ?? new MarketItem(item.Id);
	}

	public static Price GetAvg24hMarketPrice(this Item item)
	{
		var total = item.GetMarketItem().Avg24hPrice;
		if (item is CompoundItem itemC) total += itemC.Slots.Sum(slot => slot.ContainedItem?.GetAvg24hMarketPrice().Value ?? 0);
		return new Price(total);
	}

	public static Price GetAvg24hMarketPricePerSlot(this Item item)
	{
		var price = GetAvg24hMarketPrice(item).Value;
		var size = item.GetSlotSize();
		return new Price(price / (size.width * size.height));
	}

	private static Price GetTraderPrice(this Item item, string traderId)
	{
		var traderPrices = item.GetMarketItem().TraderPrices;
		var total = traderPrices?.FirstOrDefault(price => price.TraderId == traderId)?.Price ?? 0;

		if (item is CompoundItem itemC) total += itemC.Slots.Sum(slot => slot.ContainedItem?.GetTraderPrice(traderId).Value ?? 0);

		return new Price(total);
	}

	public static Price GetFleaTaxPrice(this Item item, int quantity = 1)
	{
		int Vo = item.GetMarketItem().BasePrice;
		int Vr = item.GetAvg24hMarketPrice().Value;

		double Po;
		double Pr;

		try
		{
			Po = Math.Pow(Math.Log10(Vo / Vr), Vr < Vo ? 1.08 : 1);
			Pr = Math.Pow(Math.Log10(Vr / Vo), Vr >= Vo ? 1.08 : 1);
		}
		catch (DivideByZeroException)
		{
			return new Price(0);
		}

		float Ti = 0.05f; // Tax constant
		float Tr = 0.05f; // Tax constant

		int fee = Convert.ToInt32(Math.Round((Vo * Ti * Math.Pow(4, Po) * quantity) + (Vr * Tr * Math.Pow(4, Pr) * quantity)));

		return new Price(fee);
	}

	public static Price GetFleaVsTraderProfit(this Item item)
	{
		return new Price((item.GetAvg24hMarketPrice().Value - item.GetFleaTaxPrice().Value) - item.GetTraderPrice(item.GetBestTrader().traderId).Value);
	}

	public static Price GetFleaVsTraderProfitPerSlot(this Item item)
	{
		return new Price(item.GetFleaVsTraderProfit().Value / (item.Width * item.Height));
	}

	public static string GetBestProfitableMethod(this Item item, Scan.ItemScan itemScan)
	{
		return (item.GetAvg24hMarketPrice().Value - item.GetFleaTaxPrice().Value) > item.GetBestTrader().price.Value ? "Flea" : itemScan.TraderName;
	}

	public static (string traderId, Price price, Price pricePerSlot) GetBestTrader(this Item item)
	{
		(string traderId, Price price, Price pricePerSlot) result = ("", new Price(0), new Price(0));
		foreach (var traderId in TraderPrice.TraderIds)
		{
			var traderPrice = item.GetTraderPrice(traderId);
			var traderPricePerSlot = new Price(traderPrice.Value / (item.Width * item.Height));
			if (traderPrice.Value > result.price.Value) result = (traderId, traderPrice, traderPricePerSlot);
		}

		return result;
	}
}
