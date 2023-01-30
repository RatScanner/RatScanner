using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class StatusMessage
{
	[JsonProperty("content")]
	public string Content { get; set; }

	[JsonProperty("time")]
	public string Time { get; set; }

	[JsonProperty("type")]
	public int Type { get; set; }

	[JsonProperty("solveTime")]
	public string SolveTime { get; set; }

	[JsonProperty("statusCode")]
	public string StatusCode { get; set; }
}
