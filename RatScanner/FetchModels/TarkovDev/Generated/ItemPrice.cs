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

	[Obsolete("Use vendor instead.")]
	[JsonProperty("source")]
	public ItemSourceName? Source { get; set; }

	[Obsolete("Use vendor instead.")]
	[JsonProperty("requirements")]
	public List<PriceRequirement> Requirements { get; set; }
}
