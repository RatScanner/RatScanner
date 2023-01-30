using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class SkillLevel
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("level")]
	public double Level { get; set; }
}
