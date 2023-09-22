using RatScanner.FetchModels;
using RatScanner.FetchModels.TarkovTracker;
using RatStash;
using System.Linq;
using Item = RatStash.Item;

namespace RatScanner;

public static class ItemExtensions
{
	public static int GetTaskRemaining(this Item item, bool firOnly = false, UserProgress? progress = null)
	{
		var count = 0;
		var needed = TarkovDevAPI.GetNeededItems().tasks;
		var neededItems = needed.Where(i => i.Id == item.Id && (i.FoundInRaid || !firOnly));

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

	public static int GetHideoutRemaining(this Item item, bool firOnly = false, UserProgress? progress = null)
	{
		var count = 0;
		var needed = TarkovDevAPI.GetNeededItems().hideout;
		var neededItems = needed.Where(i => i.Id == item.Id && (i.FoundInRaid || !firOnly));

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

	public static (string traderId, Price price) GetBestTrader(this Item item)
	{
		(string traderId, Price price) result = ("", new Price(0));
		foreach (var traderId in TraderPrice.TraderIds)
		{
			var traderPrice = item.GetTraderPrice(traderId);
			if (traderPrice.Value > result.price.Value) result = (traderId, traderPrice);
		}

		return result;
	}
}
