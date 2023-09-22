using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class RequirementTask
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("task")]
	public Task Task { get; set; }
}
