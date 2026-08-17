using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.TarkovDev.Json;

public class HideoutStation {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; } = string.Empty;

	[JsonProperty("areaType")]
	public int AreaType { get; set; }

	[JsonProperty("levels")]
	public List<HideoutStationLevel> Levels { get; set; } = new();

	[JsonProperty("imageLink")]
	public string? ImageLink { get; set; }
}
