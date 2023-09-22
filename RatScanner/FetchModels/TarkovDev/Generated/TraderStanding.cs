using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TraderStanding
{
	[JsonProperty("trader")]
	public Trader Trader { get; set; }

	[JsonProperty("standing")]
	public double Standing { get; set; }
}
