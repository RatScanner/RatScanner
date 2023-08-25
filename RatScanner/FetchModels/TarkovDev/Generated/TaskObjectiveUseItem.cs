using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskObjectiveUseItem : ITaskObjective
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

	[JsonProperty("useAny")]
	public List<Item> UseAny { get; set; }

	[JsonProperty("compareMethod")]
	public string CompareMethod { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("zoneNames")]
	public List<string> ZoneName { get; set; }

	public List<NeededItem> GetNeededItems(string taskId)
	{
		if (UseAny == null) return new List<NeededItem>();
		return
			UseAny.ConvertAll(x =>
			new NeededItem()
			{
				Id = x.Id,
				Count = 1,
				FoundInRaid = true,
				ProgressId = Id,
				TaskId = taskId,
			});
	}
}
