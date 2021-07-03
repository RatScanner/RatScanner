using System.Linq;
using RatScanner.FetchModels;
using RatScanner.Models;
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

		public static void SetWishlistAmount(this Item item, int value)
		{
			var wishlistItem = RatScannerMain.Instance.WishlistDB.FirstOrDefault(x => x.ItemID == item.Id);

			if (wishlistItem != null)
			{
				if (value > 0)
				{
					wishlistItem.Amount = value;
				}
				else
				{
					RatScannerMain.Instance.WishlistDB.Remove(wishlistItem);
				}
			}
			else
			{
				RatScannerMain.Instance.WishlistDB.Add(new WishlistModel
				{
					ItemID = item.Id,
					Amount = value
				});
			}
		}

		public static int GetWishlistAmount(this Item item)
		{
			return RatScannerMain.Instance.WishlistDB?.FirstOrDefault(x => x.ItemID == item.Id)?.Amount ?? 0;
		}
	}
}
