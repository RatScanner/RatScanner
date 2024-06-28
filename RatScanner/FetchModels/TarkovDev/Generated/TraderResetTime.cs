using System;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TraderResetTime
{
	[Obsolete("Use Trader.name type instead.")]
	[JsonProperty("name")]
	public string Name { get; set; }

	[Obsolete("Use Trader.resetTime type instead.")]
	[JsonProperty("resetTimestamp")]
	public string ResetTimestamp { get; set; }
}
