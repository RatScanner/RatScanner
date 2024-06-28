using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskStatusRequirement
{
	[JsonProperty("task")]
	public Task Task { get; set; }

	[JsonProperty("status")]
	public List<string> Status { get; set; }
}
