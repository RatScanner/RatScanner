using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class HideoutStationLevel
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("constructionTime")]
	public int ConstructionTime { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("itemRequirements")]
	public List<RequirementItem> ItemRequirements { get; set; }

	[JsonProperty("stationLevelRequirements")]
	public List<RequirementHideoutStationLevel> StationLevelRequirements { get; set; }

	[JsonProperty("skillRequirements")]
	public List<RequirementSkill> SkillRequirements { get; set; }

	[JsonProperty("traderRequirements")]
	public List<RequirementTrader> TraderRequirements { get; set; }

	[JsonProperty("tarkovDataId")]
	public int? TarkovDataId { get; set; }

	[JsonProperty("crafts")]
	public List<Craft> Crafts { get; set; }

	// Get needed items
	public List<NeededItem> GetNeededItems(string stationId)
	{
		var neededItems = new List<NeededItem>();
		foreach (var itemRequirement in ItemRequirements)
		{
			neededItems.AddRange(itemRequirement.GetNeededItems(stationId, Id));
		}

		return neededItems;
	}
}
