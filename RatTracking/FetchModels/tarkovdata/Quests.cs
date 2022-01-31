using Newtonsoft.Json;

namespace RatTracking.FetchModels.tarkovdata;

// Model representing the important parts of the tarkovdata quest data
public class Quest
{
	// The 'tarkovdata' Id of the quest
	[JsonProperty("id")]
	public int Id { get; set; }

	// The BSG UUID of the quest (when available)
	[JsonProperty("gameId")]
	public string? Uid { get; set; }

	// The name of the trader who gives the quest
	// TODO: Currently English only, tarkovdata is working towards localization support
	[JsonProperty("giver")]
	public string Giver { get; set; }

	// The title of the quest
	// TODO: Currently English only, tarkovdata is working towards localization support
	[JsonProperty("title")]
	public string Title { get; set; }

	[JsonProperty("wiki")]
	public string Wiki { get; set; }

	// The objectives of the quest
	[JsonProperty("objectives")]
	public QuestObjective[] Objectives { get; set; }
}
