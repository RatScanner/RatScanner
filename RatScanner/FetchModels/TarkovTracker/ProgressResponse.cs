using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovTracker;

public class ProgressResponse
{
	[JsonProperty("data")]
	public UserProgress UserProgress { get; set; } = new();

	[JsonProperty("meta")]
	public Metadata Meta { get; set; } = new();

	public class Metadata
	{
		[JsonProperty("self")]
		public string Self { get; set; } = "";
	}
}
