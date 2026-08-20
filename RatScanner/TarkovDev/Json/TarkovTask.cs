using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace RatScanner.TarkovDev.Json;

public class TaskPosition {
	[JsonProperty("x")]
	public double X { get; set; }

	[JsonProperty("y")]
	public double Y { get; set; }

	[JsonProperty("z")]
	public double Z { get; set; }
}

public class TaskSize {
	[JsonProperty("x")]
	public double X { get; set; }

	[JsonProperty("y")]
	public int Y { get; set; }

	[JsonProperty("z")]
	public double Z { get; set; }
}

public class TaskOutline {
	[JsonProperty("x")]
	public double X { get; set; }

	[JsonProperty("y")]
	public double Y { get; set; }

	[JsonProperty("z")]
	public double Z { get; set; }
}

public class TaskZone {
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("map")]
	public string Map { get; set; }

	[JsonProperty("position")]
	public TaskPosition Position { get; set; }

	[JsonProperty("size")]
	public TaskSize Size { get; set; }

	[JsonProperty("outline")]
	public List<TaskOutline> Outline { get; set; }

	[JsonProperty("top")]
	public double Top { get; set; }

	[JsonProperty("bottom")]
	public double Bottom { get; set; }

	[JsonProperty("terrainElevation")]
	public double TerrainElevation { get; set; }
}

public class TaskObjective {
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("optional")]
	public bool Optional { get; set; }

	[JsonProperty("zones")]
	public List<TaskZone> Zones { get; set; }

	[JsonProperty("maps")]
	public List<string> Maps { get; set; }

	[JsonProperty("items")]
	public List<string> ItemIds { get; set; }

	[JsonProperty("foundInRaid")]
	public bool FoundInRaid { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("markerItem")]
	public string MarkerItemId { get; set; }

	[JsonProperty("item")]
	public string BuildItemId { get; set; }
}

public class TaskStartRewards {
	[JsonProperty("traderStanding")]
	public List<object> TraderStanding { get; set; }

	[JsonProperty("items")]
	public List<object> Items { get; set; }

	[JsonProperty("offerUnlock")]
	public List<object> OfferUnlock { get; set; }

	[JsonProperty("skillLevelReward")]
	public List<object> SkillLevelReward { get; set; }

	[JsonProperty("traderUnlock")]
	public List<object> TraderUnlock { get; set; }

	[JsonProperty("craftUnlock")]
	public List<TaskCraftUnlock> CraftUnlock { get; set; }

	[JsonProperty("achievement")]
	public List<object> Achievement { get; set; }

	[JsonProperty("customization")]
	public List<TaskCustomization> Customization { get; set; }

	[JsonProperty("traderDialogueUnlock")]
	public List<object> TraderDialogueUnlock { get; set; }

	[JsonProperty("locationUnlock")]
	public List<object> LocationUnlock { get; set; }
}

public class TaskTraderStanding {
	[JsonProperty("standing")]
	public double Standing { get; set; }

	[JsonProperty("trader")]
	public string TraderId { get; set; }
	public Trader? Trader => TarkovDevAPI.GetTraders().FirstOrDefault(t => t.Id == TraderId);
}

public class TaskAttributes {

}

public class TaskItems {
	[JsonProperty("item")]
	public string Item { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("attributes")]
	public TaskAttributes Attributes { get; set; }
}

public class TaskFinishReward {
	[JsonProperty("traderStanding")]
	public List<TaskTraderStanding> TraderStanding { get; set; }

	[JsonProperty("items")]
	public List<TaskItems> Items { get; set; }

	[JsonProperty("offerUnlock")]
	public List<object> OfferUnlock { get; set; }

	[JsonProperty("skillLevelReward")]
	public List<object> SkillLevelReward { get; set; }

	[JsonProperty("traderUnlock")]
	public List<object> TraderUnlock { get; set; }

	[JsonProperty("craftUnlock")]
	public List<TaskCraftUnlock> CraftUnlock { get; set; }

	[JsonProperty("achievement")]
	public List<object> Achievement { get; set; }

	[JsonProperty("customization")]
	public List<TaskCustomization> Customization { get; set; }

	[JsonProperty("traderDialogueUnlock")]
	public List<object> TraderDialogueUnlock { get; set; }

	[JsonProperty("locationUnlock")]
	public List<object> LocationUnlock { get; set; }
}

public class TaskFailureOutcome {
	[JsonProperty("traderStanding")]
	public List<TaskTraderStanding> TraderStanding { get; set; }

	[JsonProperty("items")]
	public List<object> Items { get; set; }

	[JsonProperty("offerUnlock")]
	public List<object> OfferUnlock { get; set; }

	[JsonProperty("skillLevelReward")]
	public List<object> SkillLevelReward { get; set; }

	[JsonProperty("traderUnlock")]
	public List<object> TraderUnlock { get; set; }

	[JsonProperty("craftUnlock")]
	public List<TaskCraftUnlock> CraftUnlock { get; set; }

	[JsonProperty("achievement")]
	public List<object> Achievement { get; set; }

	[JsonProperty("customization")]
	public List<TaskCustomization> Customization { get; set; }

	[JsonProperty("traderDialogueUnlock")]
	public List<object> TraderDialogueUnlock { get; set; }

	[JsonProperty("locationUnlock")]
	public List<object> LocationUnlock { get; set; }
}

public class TaskCraftUnlock {
	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("station")]
	public string Station { get; set; }

	[JsonProperty("item")]
	public string Item { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }
}

public class TaskCustomization {
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("imageLink")]
	public string ImageLink { get; set; }

	[JsonProperty("customizationType")]
	public string CustomizationType { get; set; }

	[JsonProperty("customizationTypeName")]
	public string CustomizationTypeName { get; set; }
}

public class TaskOtherRequirement {
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("traders")]
	public List<string> Traders { get; set; }
}

public class TarkovTask
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("trader")]
	public string TraderId { get; set; }
	public Trader? Trader => TarkovDevAPI.GetTraders().FirstOrDefault(t => t.Id == TraderId);

	[JsonProperty("wikiLink")]
	public string WikiLink { get; set; }

	[JsonProperty("minPlayerLevel")]
	public int MinPlayerLevel { get; set; }

	[JsonProperty("taskRequirements")]
	public List<TaskRequirement> TaskRequirements { get; set; }

	[JsonProperty("traderRequirements")]
	public List<TaskTraderRequirement> TraderRequirements { get; set; }

	[JsonProperty("objectives")]
	public List<TaskObjective> Objectives { get; set; }

	[JsonProperty("failConditions")]
	public List<object> FailConditions { get; set; }

	[JsonProperty("startRewards")]
	public TaskStartRewards StartRewards { get; set; }

	[JsonProperty("finishRewards")]
	public TaskFinishReward FinishRewards { get; set; }

	[JsonProperty("failureOutcome")]
	public TaskFailureOutcome FailureOutcome { get; set; }

	[JsonProperty("restartable")]
	public bool Restartable { get; set; }

	[JsonProperty("experience")]
	public int Experience { get; set; }

	[JsonProperty("factionName")]
	public string FactionName { get; set; }

	[JsonProperty("neededKeys")]
	public List<TaskNeededKey> NeededKeys { get; set; }

	[JsonProperty("availableDelaySecondsMin")]
	public int AvailableDelaySecondsMin { get; set; }

	[JsonProperty("availableDelaySecondsMax")]
	public int AvailableDelaySecondsMax { get; set; }

	[JsonProperty("otherRequirements")]
	public List<TaskOtherRequirement> OtherRequirements { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("kappaRequired")]
	public bool KappaRequired { get; set; }

	[JsonProperty("lightkeeperRequired")]
	public bool LightkeeperRequired { get; set; }

	[JsonProperty("taskImageLink")]
	public string TaskImageLink { get; set; }

	[JsonProperty("map")]
	public string Map { get; set; }

	public bool HasUnmodeledRequirements => OtherRequirements is { Count: > 0 } || AvailableDelaySecondsMax is > 0 || LightkeeperRequired == true;
}

public class TaskTraderRequirement {
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("requirementType")]
	public string RequirementType { get; set; }

	[JsonProperty("compareMethod")]
	public string CompareMethod { get; set; }

	[JsonProperty("value")]
	public int Value { get; set; }

	[JsonProperty("trader")]
	public string TraderId { get; set; }
	public Trader? Trader => TarkovDevAPI.GetTraders().FirstOrDefault(t => t.Id == TraderId);
}

public class TaskRequirement {
	[JsonProperty("task")]
	public string Task { get; set; }

	[JsonProperty("status")]
	public List<string> Status { get; set; }
}

public class TaskNeededKey {
	[JsonProperty("map")]
	public string Map { get; set; }

	[JsonProperty("keys")]
	public List<string> Keys { get; set; }
}

public class TaskCondition {
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("optional")]
	public bool Optional { get; set; }

	[JsonProperty("playerLevel")]
	public int PlayerLevel { get; set; }

	[JsonProperty("maps")]
	public List<string> Maps { get; set; }
}

public class TaskSkillLevelReward {
	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("skill")]
	public string Skill { get; set; }
}

public class TaskReward {
	[JsonProperty("traderStanding")]
	public List<TaskTraderStanding> TraderStanding { get; set; }

	[JsonProperty("items")]
	public List<TaskItems> Items { get; set; }

	[JsonProperty("offerUnlock")]
	public List<string> OfferUnlock { get; set; }

	[JsonProperty("skillLevelReward")]
	public List<TaskSkillLevelReward> SkillLevelReward { get; set; }

	[JsonProperty("traderUnlock")]
	public List<string> TraderUnlock { get; set; }

	[JsonProperty("craftUnlock")]
	public List<TaskCraftUnlock> CraftUnlock { get; set; }

	[JsonProperty("achievement")]
	public List<string> Achievement { get; set; }

	[JsonProperty("customization")]
	public List<TaskCustomization> Customization { get; set; }

	[JsonProperty("traderDialogueUnlock")]
	public List<object> TraderDialogueUnlock { get; set; }

	[JsonProperty("locationUnlock")]
	public List<object> LocationUnlock { get; set; }
}

public class TaskItemFilter {
	[JsonProperty("allowedCategories")]
	public List<string> AllowedCategories { get; set; }

	[JsonProperty("allowedItems")]
	public List<string> AllowedItems { get; set; }

	[JsonProperty("excludedItems")]
	public List<object> ExcludedItems { get; set; }

	[JsonProperty("excludedCategories")]
	public List<object> ExcludedCategories { get; set; }
}

public class TaskTransferSetting {
	[JsonProperty("gridWidth")]
	public int GridWidth { get; set; }

	[JsonProperty("gridHeight")]
	public int GridHeight { get; set; }

	[JsonProperty("itemFilters")]
	public TaskItemFilter ItemFilters { get; set; }
}

public class TaskPrestige {
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("prestigeLevel")]
	public int PrestigeLevel { get; set; }

	[JsonProperty("conditions")]
	public List<TaskCondition> Conditions { get; set; }

	[JsonProperty("rewards")]
	public TaskReward Rewards { get; set; }

	[JsonProperty("transferSettings")]
	public List<TaskTransferSetting> TransferSettings { get; set; }

	[JsonProperty("iconLink")]
	public string IconLink { get; set; }

	[JsonProperty("imageLink")]
	public string ImageLink { get; set; }
}
