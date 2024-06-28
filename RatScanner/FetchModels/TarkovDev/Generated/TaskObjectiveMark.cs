using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskObjectiveMark : ITaskObjective
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

	[JsonProperty("markerItem")]
	public Item MarkerItem { get; set; }

	public List<NeededItem> GetNeededItems(string taskId)
	{
		return new List<NeededItem>()
		{
			new NeededItem()
			{
				Id = MarkerItem.Id,
				Count = 1,
				FoundInRaid = false,
				ProgressType = ProgressType.TaskTurnin,
				ProgressId = Id,
				TaskId = taskId
			}
		};
	}
}
