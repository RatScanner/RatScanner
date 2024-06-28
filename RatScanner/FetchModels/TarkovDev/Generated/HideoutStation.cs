using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class HideoutStation
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("levels")]
	public List<HideoutStationLevel> Levels { get; set; }

	[JsonProperty("tarkovDataId")]
	public int? TarkovDataId { get; set; }

	[JsonProperty("crafts")]
	public List<Craft> Crafts { get; set; }

	// Get needed items from station levels
	public List<NeededItem> GetNeededItems()
	{
		var neededItems = new List<NeededItem>();
		foreach (var level in Levels)
		{
			neededItems.AddRange(level.GetNeededItems(Id));
		}

		return neededItems;
	}
}
