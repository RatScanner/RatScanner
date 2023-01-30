using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class BossEscortAmount
{
	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("chance")]
	public double Chance { get; set; }
}
