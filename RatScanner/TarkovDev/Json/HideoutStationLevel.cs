using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.TarkovDev.Json;

public class HideoutStationLevel {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("constructionTime")]
	public int ConstructionTime { get; set; }

	[JsonProperty("traderRequirements")]
	public List<object> TraderRequirements { get; set; } = new();

	[JsonProperty("stationLevelRequirements")]
	public List<object> StationLevelRequirements { get; set; } = new();

	[JsonProperty("itemRequirements")]
	public List<RequirementItem> ItemRequirements { get; set; } = new();

	[JsonProperty("skillRequirements")]
	public List<object> SkillRequirements { get; set; } = new();

	[JsonProperty("bonuses")]
	public List<object> Bonuses { get; set; } = new();
}
