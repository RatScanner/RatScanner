using Newtonsoft.Json;

namespace RatTracking.FetchModels.TarkovTracker;

// Model representing the progress data of a TarkovTracker user
public class Progress
{
	// Whether this instance of progress is the API requestee (not a teammate)
	[JsonProperty("self")]
	public bool? Self { get; set; }

	// TarkovTracker Data Version
	[JsonProperty("dataVersion")]
	public int DataVersion { get; set; }

	// TarkovTracker Data Version
	[JsonProperty("shareName")]
	public string? DisplayName { get; set; }

	// Whether this team member is hidden
	[JsonProperty("hide")]
	public bool? Hide { get; set; }

	// TarkovTracker Quest Completion Data
	[JsonProperty("quests")]
	public Dictionary<string, QuestCompletion> Quests { get; set; }

	// TarkovTracker Quest Objective Completion Data
	[JsonProperty("objectives")]
	public Dictionary<string, QuestObjectiveCompletion> QuestObjectives { get; set; }

	// TarkovTracker Hideout Module Completion Data
	[JsonProperty("hideout")]
	public Dictionary<string, HideoutCompletion> Hideout { get; set; }

	// TarkovTracker Hideout Module Completion Data
	[JsonProperty("hideoutObjectives")]
	public Dictionary<string, HideoutObjectiveCompletion> HideoutObjectives { get; set; }
}
