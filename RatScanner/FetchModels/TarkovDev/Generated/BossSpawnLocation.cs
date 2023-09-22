using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class BossSpawnLocation
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("chance")]
	public double Chance { get; set; }
}
