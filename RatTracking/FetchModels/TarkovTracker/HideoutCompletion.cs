using Newtonsoft.Json;

namespace RatTracking.FetchModels.TarkovTracker;

// Model representing the progress data of hideout module completions
public class HideoutCompletion
{
	// Whether this hideout module is marked as complete or not
	[JsonProperty("complete")]
	public bool? Complete { get; set; }
}
