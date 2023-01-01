using RatScanner.FetchModels;
using RatStash;
using RatTracking.FetchModels;
using RatTracking.FetchModels.TarkovTracker;
using System.Collections.Generic;
using System.Linq;

namespace RatScanner;

public static class ItemExtensions
{
	// This is probably redundant, or can be simplified down based upon what the
	// output of this information ends up being
	public static bool IsProgressionItem(this Item item)
	{
		return RatScannerMain.Instance.ProgressDB.IsProgressionItem(item.Id);
	}

	public static bool IsUsedInQuest(this Item item)
	{
		return RatScannerMain.Instance.ProgressDB.IsQuestItem(item.Id);
	}

	public static bool IsUsedInHideout(this Item item)
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

	public static NeededItem GetTrackingNeeds(this Item item)
	{
		var requiredQuest = item.GetQuestRequired();
		var requiredHideout = item.GetHideoutRequired();

		var neededItem = new NeededItem(item.Id);

		UserProgress progress = null;
		if (RatConfig.Tracking.TarkovTracker.Enable && RatScannerMain.Instance.TarkovTrackerDB.Progress.Count >= 1)
		{
			var db = RatScannerMain.Instance.TarkovTrackerDB;
			progress = db.Progress.First(x => x.UserId == db.Self);
		}

		// Set this item as for ourselves if this was our token
		neededItem.Self = true;

		var (questNeeded, questHave, fir) = GetQuestRequired(requiredQuest, progress);
		neededItem.QuestNeeded += questNeeded;
		neededItem.QuestHave += questHave;
		neededItem.FIR = fir;

		var (hideoutNeeded, hideoutHave) = GetHideoutRequired(requiredHideout, progress);
		neededItem.HideoutNeeded += hideoutNeeded;
		neededItem.HideoutHave += hideoutHave;
		return neededItem;
	}

	public static List<KeyValuePair<string, NeededItem>> GetTrackingTeamNeeds(this Item item)
	{
		if (!RatConfig.Tracking.TarkovTracker.Enable) return null;

		var requiredQuest = item.GetQuestRequired();
		var requiredHideout = item.GetHideoutRequired();

		var teamProgress = RatScannerMain.Instance.TarkovTrackerDB.Progress;

		var trackedNeeds = new Dictionary<string, NeededItem>();

		foreach (var memberProgress in teamProgress)
		{
			var neededItem = new NeededItem(item.Id);
			var (questNeeded, questHave, fir) = GetQuestRequired(requiredQuest, memberProgress);
			neededItem.QuestNeeded += questNeeded;
			neededItem.QuestHave += questHave;
			neededItem.FIR = fir;

			var (hideoutNeeded, hideoutHave) = GetHideoutRequired(requiredHideout, memberProgress);
			neededItem.HideoutNeeded += hideoutNeeded;
			neededItem.HideoutHave += hideoutHave;

			var name = memberProgress.DisplayName ?? "Unknown";
			for (var i = 2; i < 99; i++)
			{
				if (!trackedNeeds.ContainsKey(name)) break;
				name = $"{memberProgress.DisplayName} #{i}";
			}

			trackedNeeds.Add(name, neededItem);
		}

		return trackedNeeds.OrderBy(x => -x.Value.Remaining).ToList();
	}

	public static NeededItem GetSummedTrackingTeamNeeds(this Item item)
	{
		var result = new NeededItem(item.Id);
		var teamData = item.GetTrackingTeamNeeds();
		if (teamData == null)
		{
			result.HideoutNeeded = 0;
			result.QuestNeeded = 0;
			result.HideoutHave = 0;
			result.QuestHave = 0;
			return result;
		}

		foreach (var (_, value) in teamData)
		{
			result.HideoutNeeded += value.HideoutNeeded;
			result.QuestNeeded += value.QuestNeeded;
			result.HideoutHave += value.HideoutHave;
			result.QuestHave += value.QuestHave;
		}

		return result;
	}

	private static (int need, int have, bool FIR) GetQuestRequired(IEnumerable<QuestItem> requiredQuestItems, UserProgress progress)
	{
		var need = 0;
		var have = 0;
		var fir = false;

		// Add up all the quest requirements
		foreach (var requirement in requiredQuestItems)
			// Add the item if its FIR or we want to show non FIR
			if (requirement.FIR || RatConfig.Tracking.ShowNonFIRNeeds)
			{
				// Update FIR flag to true if any quest is FIR
				if (requirement.FIR) fir = true;

				if (progress == null || progress.TaskObjectives == null)
				{
					need += requirement.Needed;
					continue;
				}

				// Check if the requirement task objective id exists in progress.TaskObjectives
				var taskObjective = progress.TaskObjectives.Find(x => x.Id == requirement.TaskObjectiveId);
				if (taskObjective == null)
				{
					need += requirement.Needed;
				}else
				{
					need += requirement.Needed;
					have += taskObjective.Count;
				}
			}

		return (need, have, fir);
	}

	private static (int need, int have) GetHideoutRequired(IEnumerable<HideoutItem> requiredHideoutItems, UserProgress progress)
	{
		var need = 0;
		var have = 0;

		// Add up all the hideout requirements
		foreach (var requirement in requiredHideoutItems)
		{
			if (progress == null || progress.HideoutParts == null)
			{
				need += requirement.Needed;
				continue;
			}

			// Check if the requirement hideout part id exists in progress.HideoutParts
			var hideoutPart = progress.HideoutParts.Find(x => x.Id == requirement.HideoutPartId);
			if (hideoutPart == null)
			{
				need += requirement.Needed;
			}
			else
			{
				need += requirement.Needed;
				have += hideoutPart.Count;
			}
		}

		return (need, have);
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
}
