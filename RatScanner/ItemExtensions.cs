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

				// Set this item as for ourselves if this was our token
				if (teammate.Self.GetValueOrDefault()) neededItem.Self = true;

				// Do our settings say to show quest needs?
				if(RatConfig.Tracking.ShowQuestNeeds)
				{
					// Add up all the quest requirements
					foreach (QuestItem requirement in requiredQ)
					{
						// Don't add the item to needs if its not a FIR requirement and we aren't showing non-FIR according to settings
						if (!(!RatConfig.Tracking.ShowQuestHandoverNeeds && !requirement.FIR))
						{
							// Update FIR flag to true if any quest is FIR
							if (requirement.FIR) neededItem.FIR = true;

							// If the progress data doesn't have this requirement, then it should be a needed item
							if (!teammate.QuestObjectives.ContainsKey(requirement.QuestObjectiveId.ToString()))
							{
								neededItem.QuestNeeded += requirement.Needed;
							}
							// Else if we have the requirement in our progress data but its not complete, it might have metadata
							else if (teammate.QuestObjectives[requirement.QuestObjectiveId.ToString()].Complete != true)
							{
								// Check if we have completed this quest need
								neededItem.QuestNeeded += requirement.Needed;
								neededItem.QuestHave += teammate.QuestObjectives[requirement.QuestObjectiveId.ToString()].Have ?? 0;
							}
						}
					}
				}

				// Do our settings say to show hideout needs?
				if(RatConfig.Tracking.ShowHideoutNeeds)
				{
					// Add up all the hideout requirements
					foreach (HideoutItem requirement in requiredH)
					{
						// If the progress data doesn't have this requirement, then it should be a needed item
						if (!teammate.HideoutObjectives.ContainsKey(requirement.HideoutObjectiveId.ToString()))
						{
							neededItem.HideoutNeeded += requirement.Needed;
						}
						// Else if we have the requirement in our progress data but its not complete, it might have metadata
						else if (teammate.HideoutObjectives[requirement.HideoutObjectiveId.ToString()].Complete != true)
						{
							// Check if we have completed this hideout need
							neededItem.HideoutNeeded += requirement.Needed;
							neededItem.HideoutHave += teammate.HideoutObjectives[requirement.HideoutObjectiveId.ToString()].Have ?? 0;
						}
					}
				}

				// Add to our team dictionary
				trackedNeeds.Add(teammate.DisplayName, neededItem);
			}

			// Only show ourself if ShowTeam is unchecked
			if (!RatConfig.Tracking.TarkovTracker.ShowTeam)
			{
				trackedNeeds = trackedNeeds.Where(teammate => teammate.Value.Self).ToDictionary(teammate => teammate.Key, teammate => teammate.Value);
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
