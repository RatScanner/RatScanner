using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskObjectiveItem : ITaskObjective
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("maps")]
	public List<Map> Maps { get; set; }

	[JsonProperty("optional")]
	public bool Optional { get; set; }

	[JsonProperty("item")]
	public Item Item { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("foundInRaid")]
	public bool FoundInRaid { get; set; }

	[JsonProperty("dogTagLevel")]
	public int? DogTagLevel { get; set; }

	[JsonProperty("maxDurability")]
	public int? MaxDurability { get; set; }

	[JsonProperty("minDurability")]
	public int? MinDurability { get; set; }

	public List<NeededItem> GetNeededItems(string taskId)
	{
		// If type is 'giveItem' return the needed item, else return an empty list
		if (Type != "giveItem") return new List<NeededItem>();

		return new List<NeededItem>()
		{
			new NeededItem()
			{
				Id = Item.Id,
				Count = Count,
				FoundInRaid = FoundInRaid,
				DogTagLevel = DogTagLevel,
				MinDurability = MinDurability,
				MaxDurability = MaxDurability,
				ProgressType = ProgressType.TaskTurnin,
				ProgressId = Id,
				TaskId = taskId,
			},
		};
	}
}
