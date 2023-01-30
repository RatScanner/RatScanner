using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class PriceRequirement
{
	[JsonProperty("type")]
	public RequirementType Type { get; set; }

	[JsonProperty("value")]
	public int? Value { get; set; }

	[JsonProperty("stringValue")]
	public string StringValue { get; set; }
}
