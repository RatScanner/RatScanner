using Newtonsoft.Json;

namespace RatScanner.TarkovDev.Json;

public class ContainedItem {
	[JsonProperty("item")]
	public string Item { get; set; } = string.Empty;

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("attributes")]
	public System.Collections.Generic.Dictionary<string, object>? Attributes { get; set; }
}
