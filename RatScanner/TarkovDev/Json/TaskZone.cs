using Newtonsoft.Json;

namespace RatScanner.TarkovDev.Json;

public class TaskZonePosition {
	[JsonProperty("x")]
	public float X { get; set; }

	[JsonProperty("y")]
	public float Y { get; set; }

	[JsonProperty("z")]
	public float Z { get; set; }
}

public class TaskZone {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("map")]
	public string? Map { get; set; }

	[JsonProperty("position")]
	public TaskZonePosition? Position { get; set; }

	[JsonProperty("size")]
	public TaskZonePosition? Size { get; set; }

	[JsonProperty("top")]
	public float? Top { get; set; }

	[JsonProperty("bottom")]
	public float? Bottom { get; set; }
}
