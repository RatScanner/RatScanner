using Newtonsoft.Json;

namespace RatTracking.FetchModels.tarkovdata;

// Model representing the important parts of the tarkovdata hideout data
public class HideoutData
{
	// The hideout stations
	[JsonProperty("stations")]
	public Station[] Stations { get; set; }

	// The individual hideout modules
	[JsonProperty("modules")]
	public Module[] Modules { get; set; }
}
