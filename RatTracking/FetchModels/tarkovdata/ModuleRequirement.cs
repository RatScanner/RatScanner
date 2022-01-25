using Newtonsoft.Json;

namespace RatTracking.FetchModels.tarkovdata;

// Model representing the important parts of the tarkovdata hideout module data
public class ModuleRequirement
{
	// The 'tarkovdata' Id of the module
	[JsonProperty("id")]
	public int Id { get; set; }

	// The 'type' of requirement this is, eg 'item', 'module', 'skill'
	[JsonProperty("type")]
	public string Type { get; set; }

	// The name of the required thing, similar to a target
	[JsonProperty("name")]
	public string Name { get; set; }

	// The quantity of the requirement or target needed
	[JsonProperty("quantity")]
	public int Quantity { get; set; }
}
