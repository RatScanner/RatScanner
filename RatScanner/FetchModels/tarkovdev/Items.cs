using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.tarkovdev;

// Needed Item
public class NeededItem
{
	// Item id
	[JsonProperty("id")]
	public string Id { get; set; }

	// Item count
	[JsonProperty("count")]
	public int Count { get; set; }

	// Found in raid
	[JsonProperty("foundInRaid")]
	public bool FoundInRaid { get; set; }

	// Task ID
	[JsonProperty("taskId")]
	public string? TaskId { get; set; } = null;

	// Module ID
	[JsonProperty("moduleId")]
	public string? ModuleId { get; set; } = null;

	// Station ID
	[JsonProperty("stationId")]
	public string? StationId { get; set; } = null;

	// Objective ID
	[JsonProperty("progressId")]
	public string? ProgressId { get; set; } = null;

	// Progress Type
	public ProgressType ProgressType { get; set; }
	
	// Dog tag level
	[JsonProperty("dogTagLevel")]
	public int? DogTagLevel { get; set; } = null;

	// Min durability
	[JsonProperty("minDurability")]
	public int? MinDurability { get; set; } = 0;

	// Max durability
	[JsonProperty("maxDurability")]
	public int? MaxDurability { get; set; } = 100;

	public bool HasAlternatives { get; set; } = false;
}

// Item
public class Item
{
	// Item id
	[JsonProperty("id")]
	public string Id { get; set; } = "";
}

public enum ProgressType
{
	TaskTurnin,
	TaskKey,
	TaskUse,
	HideoutTurnin
}
