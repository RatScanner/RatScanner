using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskObjectiveSkill : ITaskObjective
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

	[JsonProperty("skillLevel")]
	public SkillLevel SkillLevel { get; set; }

	public List<NeededItem> GetNeededItems(string taskId) => new();
}
