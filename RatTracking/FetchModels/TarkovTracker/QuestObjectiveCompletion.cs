using Newtonsoft.Json;

namespace RatTracking.FetchModels.TarkovTracker;

// Model representing the progress data of quest objective completions
public class QuestObjectiveCompletion
{
	// Whether this quest objective is marked as complete or not
	[JsonProperty("complete")]
	public bool? Complete { get; set; }

	// How much of this objective is complete
	[JsonProperty("have")]
	public int? Have { get; set; }
}
