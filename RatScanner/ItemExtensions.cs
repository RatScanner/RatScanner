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

	public static int GetAvg24hMarketPrice(this Item item)
	{
		var total = item.GetMarketItem().Avg24hPrice;
		if (item is CompoundItem itemC) total += itemC.Slots.Sum(slot => slot.ContainedItem?.GetAvg24hMarketPrice() ?? 0);
		return total;
	}

	public static int GetMaxTraderPrice(this Item item)
	{
		var traderPrices = item.GetMarketItem().TraderPrices;
		var total = traderPrices.Length > 0 ? traderPrices.Max(trader => trader.Price) : 0;
		if (item is CompoundItem itemC) total += itemC.Slots.Sum(slot => slot.ContainedItem?.GetMaxTraderPrice() ?? 0);
		return total;
	}

	private static int GetTraderPrice(this Item item, string traderId)
	{
		var traderPrices = item.GetMarketItem().TraderPrices;
		var total = traderPrices?.FirstOrDefault(price => price.TraderId == traderId)?.Price ?? 0;

		if (item is CompoundItem itemC) total += itemC.Slots.Sum(slot => slot.ContainedItem?.GetTraderPrice(traderId) ?? 0);

		return total;
	}

	public static (string traderId, int price) GetBestTrader(this Item item)
	{
		(string traderId, int price) result = ("", 0);
		foreach (var traderId in TraderPrice.TraderIds)
		{
			var traderPrice = item.GetTraderPrice(traderId);
			if (traderPrice > result.price) result = (traderId, traderPrice);
		}

		return result;
	}

	public static string GetBestTradeIconLink(this Item item)
	{
		var name = GetBestTrader(item).traderId switch
		{
			"54cb57776803fa99248b456e" => "Therapist",
			"58330581ace78e27b8b10cee" => "Skier",
			"5935c25fb3acc3127c3d8cd9" => "Peacekeeper",
			"54cb50c76803fa8b248b4571" => "Prapor",
			"579dc571d53a0658a154fbec" => "Fence",
			"5a7c2eca46aef81a7ca2145d" => "Mechanic",
			"5ac3b934156ae10c4430e83c" => "Ragman",
			"5c0647fdd443bc2504c2d371" => "Jaeger",
			"" => "",
			_ => "Unknown",
		};
		return $"https://tarkov.dev/images/{name.ToLower()}-icon.jpg";
	}
}
