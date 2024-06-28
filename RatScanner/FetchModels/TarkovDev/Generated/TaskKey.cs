using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskKey
{
	[JsonProperty("keys")]
	public List<Item> Keys { get; set; }

	[JsonProperty("map")]
	public Map Map { get; set; }

	public List<NeededItem> GetNeededItems(string taskId)
	{
		return Keys.Select(x => new NeededItem()
		{
			Id = x.Id,
			Count = 1,
			FoundInRaid = false,
			HasAlternatives = Keys.Count > 1,
			ProgressType = ProgressType.TaskKey,
			TaskId = taskId
		}).ToList();
	}
}
