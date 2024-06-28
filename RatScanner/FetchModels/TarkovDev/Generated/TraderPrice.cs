using System;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TraderPrice
{
	[Obsolete("Use item.buyFor instead.")]
	[JsonProperty("price")]
	public int Price { get; set; }

	[Obsolete("Use item.buyFor instead.")]
	[JsonProperty("currency")]
	public string Currency { get; set; }

	[Obsolete("Use item.buyFor instead.")]
	[JsonProperty("priceRUB")]
	public int PriceRub { get; set; }

	[Obsolete("Use item.buyFor instead.")]
	[JsonProperty("trader")]
	public Trader Trader { get; set; }
}
