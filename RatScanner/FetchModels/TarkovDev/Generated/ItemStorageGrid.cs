using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemStorageGrid
{
	[JsonProperty("width")]
	public int Width { get; set; }

	[JsonProperty("height")]
	public int Height { get; set; }

	[JsonProperty("filters")]
	public ItemFilters Filters { get; set; }
}
