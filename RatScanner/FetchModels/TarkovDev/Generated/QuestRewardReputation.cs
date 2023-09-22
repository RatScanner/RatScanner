using System;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class QuestRewardReputation
{
	[Obsolete("Use Task type instead.")]
	[JsonProperty("trader")]
	public Trader Trader { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("amount")]
	public double Amount { get; set; }
}
