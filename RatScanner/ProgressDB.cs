using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RatScanner.FetchModels;
using RatScanner.FetchModels.tarkovdata;

namespace RatScanner
{
	// Storing information about items needed for EFT progression
	public class ProgressDB
	{
		private static readonly String[] _questObjectiveItemTypes = { "collect", "find", "mark", "key" };

		// Items required for quests, including tools, handover items, and found-in-raid
		private List<QuestItem> _questItems = new List<QuestItem>();

		// Items required for hideout module upgrades
		private List<HideoutItem> _hideoutItems = new List<HideoutItem>();

		// Set up the Item DB
		public void Init()
		{
			var questBlob = ApiManager.GetProgressDataQuest();
			var hideoutBlob = ApiManager.GetProgressDataHideout();

			// Deserialize the quests.json schema into RatScanner.FetchModels.tarkovdata model
			List<Quest> quests = JsonConvert.DeserializeObject<List<Quest>>(questBlob);

			// Deserialize the hideout.json schema into RatScanner.FetchModels.tarkovdata model
			HideoutData hideout = JsonConvert.DeserializeObject<HideoutData>(hideoutBlob);

			// Loop through each quest in our quest list
			foreach (Quest quest in quests)
			{
				// Loop through all of our objectives within each quest
				foreach (QuestObjective objective in quest.Objectives)
				{
					// Check if the objective is a type that requires an item
					if (_questObjectiveItemTypes.Contains(objective.Type))
					{
						// Some objectives can have array of targets (one-of-keys are currently only example), so add each
						foreach (String item in objective.Target)
						{
							_questItems.Add(new QuestItem { Id = item, QuestId = quest.Id, Needed = objective.Number, QuestObjectiveId = objective.Id, FIR = (objective.Type == "find") });
						}
					}
				}
			}

			// Loop through each hideout module in our hideout data
			foreach (Module module in hideout.Modules)
			{
				// Loop through each requirement within the module
				foreach (ModuleRequirement requirement in module.Requirements)
				{
					// If its an item requirement, add it to the list
					if (requirement.Type == "item")
					{
						_hideoutItems.Add(new HideoutItem { Id = requirement.Name, StationId = module.StationId, ModuleLevel = module.Level, HideoutObjectiveId = requirement.Id, Needed = requirement.Quantity });
					}
				}
			}
		}

		// Check if item is needed for any part of progression, quest or hideout
		public bool IsProgressionItem(string uid)
		{
			if (uid?.Length > 0)
			{
				return (IsQuestItem(uid) || IsHideoutItem(uid));
			}
			Logger.LogWarning("Trying to get item without supplying uid");
			throw new ArgumentException();
		}

		// Quick lookup to see if the item is a required part of any quest
		public bool IsQuestItem(string uid)
		{
			if (uid?.Length > 0)
			{
				var item = _questItems.FirstOrDefault(i => i.Id == uid);
				if (item != null) return true;

				Logger.LogWarning("Could not find quest item with uid: " + uid);
				return false;
			}
			Logger.LogWarning("Trying to get item without supplying uid");
			throw new ArgumentException();
		}

		// Quick lookup to see if the item is a required part of any hideout upgrade
		public bool IsHideoutItem(string uid)
		{
			if (uid?.Length > 0)
			{
				var item = _hideoutItems.FirstOrDefault(i => i.Id == uid);
				if (item != null) return true;

				Logger.LogWarning("Could not find hideout item with uid: " + uid);
				return false;
			}
			Logger.LogWarning("Trying to get item without supplying uid");
			throw new ArgumentException();
		}

		// Return all the instances where this item is needed for quests
		public List<QuestItem> GetQuestRequiredById(string uid)
		{
			if (uid?.Length > 0)
			{
				return _questItems.Where(i => i.Id == uid).ToList();
			}
			Logger.LogWarning("Trying to get item without supplying uid");
			throw new ArgumentException();
		}

		// Return all the instances where this item is needed for hideout upgrades
		public List<HideoutItem> GetHideoutRequiredById(string uid)
		{
			if (uid?.Length > 0)
			{
				return _hideoutItems.Where(i => i.Id == uid).ToList();
			}
			Logger.LogWarning("Trying to get item without supplying uid");
			throw new ArgumentException();
		}
	}
}
