using System.Linq;
using RatScanner.FetchModels;
using RatStash;

namespace RatScanner
{
	public static class ItemExtensions
	{
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
