using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class RequirementHideoutStationLevel
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("station")]
	public HideoutStation Station { get; set; }

	[JsonProperty("level")]
	public int Level { get; set; }
}
