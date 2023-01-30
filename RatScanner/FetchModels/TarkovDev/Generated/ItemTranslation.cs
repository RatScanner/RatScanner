using System;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemTranslation
{
	[Obsolete("Use the lang argument on queries instead.")]
	[JsonProperty("name")]
	public string Name { get; set; }

	[Obsolete("Use the lang argument on queries instead.")]
	[JsonProperty("shortName")]
	public string ShortName { get; set; }

	[Obsolete("Use the lang argument on queries instead.")]
	[JsonProperty("description")]
	public string Description { get; set; }
}
