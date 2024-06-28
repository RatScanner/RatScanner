using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemAttribute
{
	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("value")]
	public string Value { get; set; }
}
