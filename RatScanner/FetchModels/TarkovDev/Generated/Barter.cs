using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Barter
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("trader")]
	public Trader Trader { get; set; }

	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("taskUnlock")]
	public Task TaskUnlock { get; set; }

	[JsonProperty("requiredItems")]
	public List<ContainedItem> RequiredItems { get; set; }

	[JsonProperty("rewardItems")]
	public List<ContainedItem> RewardItems { get; set; }

	[Obsolete("Use trader and level instead.")]
	[JsonProperty("source")]
	public string Source { get; set; }

	[Obsolete("Use trader instead.")]
	[JsonProperty("sourceName")]
	public ItemSourceName SourceName { get; set; }

	[Obsolete("Use level instead.")]
	[JsonProperty("requirements")]
	public List<PriceRequirement> Requirements { get; set; }
}
