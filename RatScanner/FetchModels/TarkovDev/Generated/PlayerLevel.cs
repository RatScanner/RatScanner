using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class PlayerLevel
{
	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("exp")]
	public int Exp { get; set; }
}
