using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class AttributeThreshold
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("requirement")]
	public NumberCompare Requirement { get; set; }
}
