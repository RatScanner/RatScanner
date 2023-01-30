using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class StimEffect
{
	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("chance")]
	public double Chance { get; set; }

	[JsonProperty("delay")]
	public int Delay { get; set; }

	[JsonProperty("duration")]
	public int Duration { get; set; }

	[JsonProperty("value")]
	public double Value { get; set; }

	[JsonProperty("percent")]
	public bool Percent { get; set; }

	[JsonProperty("skillName")]
	public string SkillName { get; set; }
}
