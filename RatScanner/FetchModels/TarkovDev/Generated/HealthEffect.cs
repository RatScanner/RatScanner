using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class HealthEffect
{
	[JsonProperty("bodyParts")]
	public List<string> BodyParts { get; set; }

	[JsonProperty("effects")]
	public List<string> Effects { get; set; }

	[JsonProperty("time")]
	public NumberCompare Time { get; set; }
}
