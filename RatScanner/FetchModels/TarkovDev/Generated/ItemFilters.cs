using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemFilters
{
	[JsonProperty("allowedCategories")]
	public List<ItemCategory> AllowedCategories { get; set; }

	[JsonProperty("allowedItems")]
	public List<Item> AllowedItems { get; set; }

	[JsonProperty("excludedCategories")]
	public List<ItemCategory> ExcludedCategories { get; set; }

	[JsonProperty("excludedItems")]
	public List<Item> ExcludedItems { get; set; }
}
