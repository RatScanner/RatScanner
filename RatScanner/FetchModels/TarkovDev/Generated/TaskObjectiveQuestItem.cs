using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskObjectiveQuestItem : ITaskObjective
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

	[JsonProperty("questItem")]
	public QuestItem QuestItem { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }

	public List<NeededItem> GetNeededItems(string taskId)
	{
		return new()
		{
			new()
			{
				Id = QuestItem.Id,
				Count = 1,
				FoundInRaid = true,
				ProgressId = Id,
				TaskId = taskId,
			},
		};
	}
}
