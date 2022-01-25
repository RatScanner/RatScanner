using Newtonsoft.Json;

namespace RatTracking.FetchModels.TarkovTracker;

// Model representing the progress data of quest completions
public class QuestCompletion
{
	// Whether this quest is marked as complete or not
	[JsonProperty("complete")]
	public bool? Complete { get; set; }
}
