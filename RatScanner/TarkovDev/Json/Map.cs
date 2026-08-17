using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.TarkovDev.Json;

public class Map {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; } = string.Empty;

	[JsonProperty("nameId")]
	public string NameId { get; set; } = string.Empty;

	[JsonProperty("description")]
	public string Description { get; set; } = string.Empty;

	[JsonProperty("scenePath")]
	public string ScenePath { get; set; } = string.Empty;

	[JsonProperty("wiki")]
	public string? Wiki { get; set; }

	[JsonProperty("enemies")]
	public List<string> Enemies { get; set; } = new();

	[JsonProperty("raidDuration")]
	public int RaidDuration { get; set; }

	[JsonProperty("players")]
	public string Players { get; set; } = string.Empty;

	[JsonProperty("bosses")]
	public List<object> Bosses { get; set; } = new();
}
