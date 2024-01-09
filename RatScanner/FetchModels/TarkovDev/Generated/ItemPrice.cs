using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPrice
{
	[JsonProperty("vendor")]
	public IVendor Vendor { get; set; }

	[JsonProperty("price")]
	public int? Price { get; set; }

	[JsonProperty("currency")]
	public string Currency { get; set; }

	[JsonProperty("currencyItem")]
	public Item CurrencyItem { get; set; }

	[JsonProperty("priceRUB")]
	public int? PriceRub { get; set; }
}
