using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class GameProperty
{
	[JsonProperty("key")]
	public string Key { get; set; }

	[JsonProperty("numericValue")]
	public double? NumericValue { get; set; }

	[JsonProperty("stringValue")]
	public string StringValue { get; set; }

	[JsonProperty("arrayValue")]
	public List<string> ArrayValue { get; set; }

	[JsonProperty("objectValue")]
	public string ObjectValue { get; set; }
}
