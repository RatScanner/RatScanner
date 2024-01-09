using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Craft
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("station")]
	public HideoutStation Station { get; set; }

	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("taskUnlock")]
	public Task TaskUnlock { get; set; }

	[JsonProperty("duration")]
	public int Duration { get; set; }

	[JsonProperty("requiredItems")]
	public List<ContainedItem> RequiredItems { get; set; }

	[JsonProperty("requiredQuestItems")]
	public List<QuestItem> RequiredQuestItems { get; set; }

	[JsonProperty("rewardItems")]
	public List<ContainedItem> RewardItems { get; set; }
}
