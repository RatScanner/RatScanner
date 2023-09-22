using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class QuestItem
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("shortName")]
	public string ShortName { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("width")]
	public int? Width { get; set; }

	[JsonProperty("height")]
	public int? Height { get; set; }

	[JsonProperty("iconLink")]
	public string IconLink { get; set; }

	[JsonProperty("gridImageLink")]
	public string GridImageLink { get; set; }

	[JsonProperty("baseImageLink")]
	public string BaseImageLink { get; set; }

	[JsonProperty("inspectImageLink")]
	public string InspectImageLink { get; set; }

	[JsonProperty("image512pxLink")]
	public string Image512PxLink { get; set; }

	[JsonProperty("image8xLink")]
	public string Image8XLink { get; set; }
}
