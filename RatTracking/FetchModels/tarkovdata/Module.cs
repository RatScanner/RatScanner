using Newtonsoft.Json;

namespace RatTracking.FetchModels.tarkovdata;

// Model representing the important parts of the tarkovdata hideout module data
public class Module
{
	// The 'tarkovdata' Id of the module
	[JsonProperty("id")]
	public int Id { get; set; }

	// The 'level' of the module (in terms of what stage the station is at, 1 being first)
	[JsonProperty("level")]
	public int Level { get; set; }

	// The Id of the station which this module is part of
	[JsonProperty("stationId")]
	public int StationId { get; set; }

	// The requirements to build this module
	[JsonProperty("require")]
	public ModuleRequirement[] Requirements { get; set; }
}
