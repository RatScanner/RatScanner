using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ContainedItem
{
	[JsonProperty("item")]
	public Item Item { get; set; }

	[JsonProperty("count")]
	public double Count { get; set; }

	[JsonProperty("quantity")]
	public double Quantity { get; set; }

	[JsonProperty("attributes")]
	public List<ItemAttribute> Attributes { get; set; }
}
