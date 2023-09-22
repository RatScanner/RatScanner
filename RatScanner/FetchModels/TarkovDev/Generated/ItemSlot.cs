using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemSlot
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("nameId")]
	public string NameId { get; set; }

	[JsonProperty("filters")]
	public ItemFilters Filters { get; set; }

	[JsonProperty("required")]
	public bool? Required { get; set; }
}
