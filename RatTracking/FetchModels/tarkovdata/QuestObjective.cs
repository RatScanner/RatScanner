using Newtonsoft.Json;

namespace RatTracking.FetchModels.tarkovdata;

// Model representing individual quest objectives from tarkovdata
public class QuestObjective
{
	// The 'tarkovdata' Id of the quest objective
	[JsonProperty("id")]
	public int Id { get; set; }

	// The 'type' of objective this is, such as kill, place, find, etc
	[JsonProperty("type")]
	public string Type { get; set; }

	// The 'number' of the objective, indicating how many of an item are needed,
	// how many times you would need to repeat an action, etc
	[JsonProperty("number")]
	public int Number { get; set; }

	// The primary objective of the objective - an item id that needs to be
	// collected, a quest item needed, or an elimintation target type
	[JsonProperty("target")]
	[JsonConverter(typeof(ObjectiveTargetOrArrayConverter<string>))]
	public List<string> Target { get; set; }
}
