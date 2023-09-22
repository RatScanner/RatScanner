using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class HistoricalPricePoint
{
	[JsonProperty("price")]
	public int? Price { get; set; }

	[JsonProperty("timestamp")]
	public string Timestamp { get; set; }
}
