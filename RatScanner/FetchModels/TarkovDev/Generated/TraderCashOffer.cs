using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TraderCashOffer
{
	[JsonProperty("item")]
	public Item Item { get; set; }

	[JsonProperty("minTraderLevel")]
	public int? MinTraderLevel { get; set; }

	[JsonProperty("price")]
	public int? Price { get; set; }

	[JsonProperty("currency")]
	public string Currency { get; set; }

	[JsonProperty("currencyItem")]
	public Item CurrencyItem { get; set; }

	[JsonProperty("priceRUB")]
	public int? PriceRub { get; set; }

	[JsonProperty("taskUnlock")]
	public Task TaskUnlock { get; set; }
}
