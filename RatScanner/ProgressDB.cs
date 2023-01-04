using Newtonsoft.Json;
using RatScanner.FetchModels;
using RatScanner.FetchModels.tarkovdata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RatScanner;

// Storing information about items needed for EFT progression
public class ProgressDB
{
	private static readonly string[] _questObjectiveItemTypes = { "collect", "find", "mark", "key" };

	private const string TarkovDataUrl = "https://tarkovtracker.github.io/tarkovdata";

	// Items required for quests, including tools, handover items, and found-in-raid
	private List<QuestItem> _questItems = new();

	// Items required for hideout module upgrades
	private List<HideoutItem> _hideoutItems = new();

	// Set up the Item DB
	public void Init()
	{
		var neededItems = TarkovDevAPI.GetNeededItems();
		// Loop through each neededItem
		neededItems.ForEach((neededItem) =>
		{
			if (neededItem.ProgressType == FetchModels.tarkovdev.ProgressType.TaskTurnin)
			{
				_questItems.Add(new QuestItem
				{
					Id = neededItem.Id,
					Needed = neededItem.Count,
					TaskId = neededItem.TaskId ?? "",
					FIR = neededItem.FoundInRaid,
					HasAlternatives = neededItem.HasAlternatives,
					TaskObjectiveId = neededItem.ProgressId ?? ""
				});
			}
			else if (neededItem.ProgressType == FetchModels.tarkovdev.ProgressType.HideoutTurnin)
			{
				_hideoutItems.Add(new HideoutItem
				{
					Id = neededItem.Id,
					Needed = neededItem.Count,
					ModuleId = neededItem.ModuleId ?? "",
					StationId = neededItem.StationId ?? "",
					HideoutPartId = neededItem.ProgressId ?? "",
				});
			}
		});
	}

	// Check if item is needed for any part of progression, quest or hideout
	public bool IsProgressionItem(string uid)
	{
		if (uid?.Length > 0) return IsQuestItem(uid) || IsHideoutItem(uid);
		throw new ArgumentException();
	}

	// Quick lookup to see if the item is a required part of any quest
	public bool IsQuestItem(string uid)
	{
		if (uid?.Length > 0)
		{
			var item = _questItems.FirstOrDefault(i => i.Id == uid);
			if (item != null) return true;

			return false;
		}

		throw new ArgumentException();
	}

	// Quick lookup to see if the item is a required part of any hideout upgrade
	public bool IsHideoutItem(string uid)
	{
		if (uid?.Length > 0)
		{
			var item = _hideoutItems.FirstOrDefault(i => i.Id == uid);
			if (item != null) return true;

			return false;
		}

		throw new ArgumentException();
	}

	// Return all the instances where this item is needed for quests
	public List<QuestItem> GetQuestRequiredById(string uid)
	{
		if (uid?.Length > 0) return _questItems.Where(i => i.Id == uid).ToList();
		throw new ArgumentException();
	}

	// Return all the instances where this item is needed for hideout upgrades
	public List<HideoutItem> GetHideoutRequiredById(string uid)
	{
		if (uid?.Length > 0) return _hideoutItems.Where(i => i.Id == uid).ToList();
		throw new ArgumentException();
	}
}
