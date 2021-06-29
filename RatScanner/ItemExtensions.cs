using System.Linq;
using RatScanner.FetchModels;
using System.Collections.Generic;
using RatStash;
using RatScanner.FetchModels.TarkovTracker;

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

		public static List<QuestItem> GetQuestRequired(this Item item)
		{
			return RatScannerMain.Instance.ProgressDB.GetQuestRequiredById(item.Id);
		}

		public static List<HideoutItem> GetHideoutRequired(this Item item)
		{
			return RatScannerMain.Instance.ProgressDB.GetHideoutRequiredById(item.Id);
		}

		public static Dictionary<string, NeededItem> GetTrackingNeeds(this Item item)
		{
			// Check if we are able to connect to TarkovTracker
			if (!RatScannerMain.Instance.TarkovTrackerDB.ValidToken())
			{
				// Our token is invalid/missing
				return null;
			}

			// Get the latest progression and tracker data
			List<QuestItem> requiredQ = item.GetQuestRequired();
			List<HideoutItem> requiredH = item.GetHideoutRequired();

			List<Progress> team = RatScannerMain.Instance.TarkovTrackerDB.GetProgress();

			// Create our team dictionary
			Dictionary<string, NeededItem> trackedNeeds = new Dictionary<string, NeededItem>();
			foreach (Progress teammate in team)
			{
				// Create the needed item for this player
				NeededItem neededItem = new NeededItem(item.Id);

				// Add up all the quest requirements
				foreach (QuestItem need in requiredQ)
				{
					// Update FIR flag to true if any quest is FIR
					if (need.FIR) neededItem.FIR = true;

					if (!teammate.QuestObjectives.ContainsKey(need.QuestObjectiveId.ToString()))
					{
						neededItem.QuestNeeded += need.Needed;
					}
					else if (teammate.QuestObjectives[need.QuestObjectiveId.ToString()].Complete != true)
					{
						// Check if we have completed this quest need
						neededItem.QuestNeeded += need.Needed;
						neededItem.QuestHave += teammate.QuestObjectives[need.QuestObjectiveId.ToString()].Have ?? 0;
					}
				}

				// Add up all the hideout requirements
				foreach (HideoutItem need in requiredH)
				{
					if (!teammate.HideoutObjectives.ContainsKey(need.HideoutObjectiveId.ToString()))
					{
						neededItem.HideoutNeeded += need.Needed;
					}
					else if (teammate.HideoutObjectives[need.HideoutObjectiveId.ToString()].Complete != true)
					{
						// Check if we have completed this hideout need
						neededItem.HideoutNeeded += need.Needed;
						neededItem.HideoutHave += teammate.HideoutObjectives[need.HideoutObjectiveId.ToString()].Have ?? 0;
					}
				}

				// Add to our team dictionary
				trackedNeeds.Add(teammate.DisplayName, neededItem);
			}
			return trackedNeeds;
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
