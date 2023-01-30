using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class RequirementSkill
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("level")]
	public int Level { get; set; }
}
