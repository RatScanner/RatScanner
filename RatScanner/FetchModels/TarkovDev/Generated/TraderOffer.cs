using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TraderOffer : IVendor
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("trader")]
	public Trader Trader { get; set; }

	[JsonProperty("minTraderLevel")]
	public int? MinTraderLevel { get; set; }

	[JsonProperty("taskUnlock")]
	public Task TaskUnlock { get; set; }
}
