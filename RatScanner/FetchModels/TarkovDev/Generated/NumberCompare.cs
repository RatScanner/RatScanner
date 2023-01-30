using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class NumberCompare
{
	[JsonProperty("compareMethod")]
	public string CompareMethod { get; set; }

	[JsonProperty("value")]
	public double Value { get; set; }
}
