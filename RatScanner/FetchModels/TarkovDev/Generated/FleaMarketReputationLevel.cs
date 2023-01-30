using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class FleaMarketReputationLevel
{
	[JsonProperty("offers")]
	public int Offers { get; set; }

	[JsonProperty("minRep")]
	public double MinRep { get; set; }

	[JsonProperty("maxRep")]
	public double MaxRep { get; set; }
}
