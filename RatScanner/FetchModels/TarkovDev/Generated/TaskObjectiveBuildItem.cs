using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskObjectiveBuildItem : ITaskObjective
{
	[JsonProperty("item")]
	public Item Item { get; set; }

	[JsonProperty("containsAll")]
	public List<Item> ContainsAll { get; set; }

	[JsonProperty("containsOne")]
	public List<Item> ContainsOne { get; set; }

	[JsonProperty("attributes")]
	public List<AttributeThreshold> Attributes { get; set; }

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

	public List<NeededItem> GetNeededItems(string taskId)
	{
		// Return the item and all items in containsAll
		return new List<NeededItem>
			{
				new()
				{
					Id = Item.Id,
					Count = 1,
					FoundInRaid = false,
					ProgressType = ProgressType.TaskTurnin,
					ProgressId = Id,
					TaskId = taskId,
				},
			}
			.Concat(ContainsAll.Select(x => new NeededItem
			{
				Id = x.Id,
				Count = 1,
				FoundInRaid = false,
				ProgressType = ProgressType.TaskTurnin,
				ProgressId = Id,
				TaskId = taskId,
			}))
			.Concat(ContainsOne.Select(x => new NeededItem
			{
				Id = x.Id,
				Count = 1,
				FoundInRaid = false,
				HasAlternatives = true,
				ProgressType = ProgressType.TaskTurnin,
				ProgressId = Id,
				TaskId = taskId,
			}))
			.ToList();
	}
}
