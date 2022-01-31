using Newtonsoft.Json;

namespace RatTracking.FetchModels.tarkovdata;

// Model representing the important parts of the tarkovdata hideout station data
public class Station
{
	// The 'tarkovdata' Id of the station
	[JsonProperty("id")]
	public int Id { get; set; }

	// The localized name of what to call this station
	[JsonProperty("locales")]
	public Dictionary<string, string> Locales { get; set; }
}
