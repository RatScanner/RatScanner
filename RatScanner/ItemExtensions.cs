using System.Linq;
using RatScanner.FetchModels;
using System.Collections.Generic;
using RatStash;

namespace RatScanner
{
	public static class ItemExtensions
	{
		// This is probably redundant, or can be simplified down based upon what the
		// output of this information ends up being
		public static bool IsProgressionItem(this Item item)
		{
			return RatScannerMain.Instance.ProgressDB.IsProgressionItem(item.Id);
		}

		public static bool IsQuestItem(this Item item)
		{
			return RatScannerMain.Instance.ProgressDB.IsQuestItem(item.Id);
		}

		public static bool IsHideoutItem(this Item item)
		{
			return RatScannerMain.Instance.ProgressDB.IsHideoutItem(item.Id);
		}

		public static List<QuestItem> GetQuestNeeds(this Item item)
		{
			return RatScannerMain.Instance.ProgressDB.GetQuestNeedsById(item.Id);
		}

		public static List<HideoutItem> GetHideoutNeeds(this Item item)
		{
			return RatScannerMain.Instance.ProgressDB.GetHideoutNeedsById(item.Id);
		}


		public static MarketItem GetMarketItem(this Item item)
		{
			var marketItem = RatScannerMain.Instance.MarketDB.GetItemById(item.Id);
			return marketItem ?? new MarketItem(item.Id);
		}

		public static int GetAvg24hMarketPrice(this Item item)
		{
			var total = item.GetMarketItem().Avg24hPrice;
			if (item is CompoundItem itemC)
			{
				total += itemC.Slots.Sum(slot => slot.ContainedItem?.GetAvg24hMarketPrice() ?? 0);
			}
			return total;
		}

		public static int GetMaxTraderPrice(this Item item)
		{
			var traderPrices = item.GetMarketItem().TraderPrices;
			var total = traderPrices.Length > 0 ? traderPrices.Max(trader => trader.Price) : 0;
			if (item is CompoundItem itemC)
			{
				total += itemC.Slots.Sum(slot => slot.ContainedItem?.GetMaxTraderPrice() ?? 0);
			}
			return total;
		}

		private static int GetTraderPrice(this Item item, string traderId)
		{
			var traderPrices = item.GetMarketItem().TraderPrices;
			var total = traderPrices?.FirstOrDefault(price => price.TraderId == traderId)?.Price ?? 0;

			if (item is CompoundItem itemC)
			{
				total += itemC.Slots.Sum(slot => slot.ContainedItem?.GetTraderPrice(traderId) ?? 0);
			}

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
	}
}
