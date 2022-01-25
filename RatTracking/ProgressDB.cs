using Newtonsoft.Json;
using RatTracking.FetchModels;
using RatTracking.FetchModels.tarkovdata;

namespace RatTracking;

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
		var questBlob = getProgressDataQuest();
		var hideoutBlob = getProgressDataHideout();

		// Deserialize the quests.json schema into RatScanner.FetchModels.tarkovdata model
		var quests = JsonConvert.DeserializeObject<List<Quest>>(questBlob);

		// Deserialize the hideout.json schema into RatScanner.FetchModels.tarkovdata model
		var hideout = JsonConvert.DeserializeObject<HideoutData>(hideoutBlob);

		// Loop through each quest in our quest list
		foreach (var quest in quests)
			// Loop through all of our objectives within each quest
		foreach (var objective in quest.Objectives)
			// Check if the objective is a type that requires an item
			if (_questObjectiveItemTypes.Contains(objective.Type))
				// Some objectives can have array of targets (one-of-keys are currently only example), so add each
				foreach (var item in objective.Target)
					_questItems.Add(new QuestItem
					{
						Id = item, QuestId = quest.Id, Needed = objective.Number, QuestObjectiveId = objective.Id, FIR = objective.Type == "find",
					});

		// Loop through each hideout module in our hideout data
		foreach (var module in hideout.Modules)
			// Loop through each requirement within the module
		foreach (var requirement in module.Requirements)
			// If its an item requirement, add it to the list
			if (requirement.Type == "item")
				_hideoutItems.Add(new HideoutItem
				{
					Id = requirement.Name, StationId = module.StationId, ModuleLevel = module.Level, HideoutObjectiveId = requirement.Id,
					Needed = requirement.Quantity,
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

	// Pulls the whole quest data file from tarkovdata for processing
	private static string getProgressDataQuest()
	{
		try
		{
			return APIClient.Get($"{TarkovDataUrl}/quests.json");
		}
		catch (Exception)
		{
			return null;
		}
	}

	// Pulls the whole hideout file form tarkovdata for processing
	private static string getProgressDataHideout()
	{
		try
		{
			return APIClient.Get($"{TarkovDataUrl}/hideout.json");
		}
		catch (Exception)
		{
			return null;
		}
	}
}
