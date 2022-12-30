using Newtonsoft.Json;

namespace RatTracking.FetchModels.TarkovTracker;

public class TeamProgressResponse
{
	[JsonProperty("data")]
	public List<UserProgress> TeamProgress { get; set; } = new();

	[JsonProperty("meta")]
	public Metadata Meta { get; set; } = new();

	public class Metadata
	{
		[JsonProperty("self")]
		public string Self { get; set; } = "";

		[JsonProperty("hiddenTeammates")]
		public List<string> HiddenTeammates { get; set; } = new();
	}
}
