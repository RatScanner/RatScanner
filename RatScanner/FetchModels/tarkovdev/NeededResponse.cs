using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.FetchModels.TarkovDev;

public class NeededResponseData
{
	[JsonProperty("tasks")]
	public List<Task> Tasks { get; set; } = new();

	// Hideout stations
	[JsonProperty("hideoutStations")]
	public List<HideoutStation> HideoutStations { get; set; } = new();

	// Get needed items from tasks and hideout station
	public (List<NeededItem> tasks, List<NeededItem> hideout) GetNeededItems()
	{
		var neededItems = (new List<NeededItem>(), new List<NeededItem>());
		foreach (var task in Tasks)
		{
			neededItems.Item1.AddRange(task.GetNeededItems());
		}
		foreach (var station in HideoutStations)
		{
			neededItems.Item2.AddRange(station.GetNeededItems());
		}
		return neededItems;
	}
}

public class NeededResponse
{
	[JsonProperty("data")]
	public NeededResponseData Data { get; set; }
}
