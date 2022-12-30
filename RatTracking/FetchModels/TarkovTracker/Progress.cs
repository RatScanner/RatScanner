using Newtonsoft.Json;

namespace RatTracking.FetchModels.TarkovTracker;

public class Progress
{
	[JsonProperty("id")]
	public string Id { get; set; } = "";

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("complete")]
	public bool Complete { get; set; }
}
