using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovTracker;

public class Progress {
	[JsonProperty("id")]
	public string Id { get; set; } = "";

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("complete")]
	public bool Complete { get; set; }


	/// <summary>Task was failed (v2 tasksProgress entries only).</summary>
	[JsonProperty("failed")]
	public bool Failed { get; set; }

	/// <summary>Progress entry no longer applies (e.g. wrong faction).</summary>
	[JsonProperty("invalid")]
	public bool Invalid { get; set; }
}
