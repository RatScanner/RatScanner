using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.TarkovDev.Json;

public class TaskObjective {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("description")]
	public string Description { get; set; } = string.Empty;

	[JsonProperty("type")]
	public string Type { get; set; } = string.Empty;

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("optional")]
	public bool Optional { get; set; }

	[JsonProperty("items")]
	public List<string> Items { get; set; } = new();

	[JsonProperty("dogTagLevel")]
	public int DogTagLevel { get; set; }

	[JsonProperty("maxDurability")]
	public int MaxDurability { get; set; }

	[JsonProperty("minDurability")]
	public int MinDurability { get; set; }

	[JsonProperty("foundInRaid")]
	public bool FoundInRaid { get; set; }

	[JsonProperty("markerItem")]
	public string? MarkerItem { get; set; }

	[JsonProperty("item")]
	public string? Item { get; set; }

	[JsonProperty("maps")]
	public List<string> Maps { get; set; } = new();

	[JsonProperty("zones")]
	public List<TaskZone> Zones { get; set; } = new();
}
