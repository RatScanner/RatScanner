using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.FetchModels.TarkovDev;

public class HideoutStation
{
	// Id
	[JsonProperty("id")]
	public string Id { get; set; }

	// Name
	[JsonProperty("name")]
	public string Name { get; set; }

	// Station levels
	[JsonProperty("levels")]
	public List<HideoutStationLevel> Levels { get; set; } = new();

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

public class HideoutStationLevel
{
	// Id
	[JsonProperty("id")]
	public string Id { get; set; }

	// Level
	[JsonProperty("level")]
	public int Level { get; set; }

	// Item Requirements
	[JsonProperty("itemRequirements")]
	public List<LevelItemRequirement> ItemRequirements { get; set; } = new();


	// Station level requirements
	[JsonProperty("stationLevelRequirements")]
	public List<LevelStationRequirement> StationLevelRequirements { get; set; } = new();

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

public class LevelItemRequirement
{
	// Id
	[JsonProperty("id")]
	public string Id { get; set; }

	// Item
	[JsonProperty("item")]
	public Item Item { get; set; }

	// Count
	[JsonProperty("count")]
	public int Count { get; set; }

	// Get needed items
	public List<NeededItem> GetNeededItems(string stationId, string moduleId)
	{
		return new List<NeededItem>
		{
			new NeededItem
			{
				Id = Item.Id,
				Count = Count,
				FoundInRaid = false,
				ProgressType = ProgressType.HideoutTurnin,
				ProgressId = Id,
				ModuleId = moduleId,
				StationId = stationId
			}
		};
	}
}

public class LevelStationRequirement
{
	// Id
	[JsonProperty("id")]
	public string Id { get; set; }

	// Station
	[JsonProperty("station")]
	public RequiredStation Station { get; set; }

	// Level
	[JsonProperty("level")]
	public int Level { get; set; }
}

public class RequiredStation
{
	// Id
	[JsonProperty("id")]
	public string Id { get; set; }

	// Name
	[JsonProperty("name")]
	public string Name { get; set; }
}
