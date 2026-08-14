using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.TarkovDev.Json;

public class RequirementItem {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("item")]
	public string Item { get; set; } = string.Empty;

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("attributes")]
	public Dictionary<string, object>? Attributes { get; set; }
}
