using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Status
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("message")]
	public string Message { get; set; }

	[JsonProperty("status")]
	public int StatusInt { get; set; }

	[JsonProperty("statusCode")]
	public string StatusCode { get; set; }
}
