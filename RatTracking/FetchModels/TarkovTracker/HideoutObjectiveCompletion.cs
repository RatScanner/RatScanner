using Newtonsoft.Json;

namespace RatTracking.FetchModels.TarkovTracker;

// Model representing the progress data of hideout objective completions
public class HideoutObjectiveCompletion
{
	// Whether this hideout objective is marked as complete or not
	[JsonProperty("complete")]
	public bool? Complete { get; set; }

	// How much of this objective is complete
	[JsonProperty("have")]
	public int? Have { get; set; }
}
