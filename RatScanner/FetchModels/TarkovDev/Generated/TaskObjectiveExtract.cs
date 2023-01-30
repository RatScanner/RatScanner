using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskObjectiveExtract : ITaskObjective
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

	[JsonProperty("exitStatus")]
	public List<string> ExitStatus { get; set; }

	[JsonProperty("exitName")]
	public string ExitName { get; set; }

	[JsonProperty("zoneNames")]
	public List<string> ZoneNames { get; set; }

	public List<NeededItem> GetNeededItems(string taskId) => new();
}
