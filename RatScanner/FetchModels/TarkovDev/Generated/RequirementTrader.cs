using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class RequirementTrader
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("trader")]
	public Trader Trader { get; set; }

	[JsonProperty("level")]
	public int Level { get; set; }
}
