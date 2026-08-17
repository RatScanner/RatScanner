using Newtonsoft.Json;
using System.Linq;

namespace RatScanner.TarkovDev.Json;

public class TraderPrice {
	[JsonProperty("trader")]
	public string TraderId { get; set; } = string.Empty;
	public Trader? Trader => TarkovDevAPI.GetTraders().FirstOrDefault(t => t.Id == TraderId);

	[JsonProperty("price")]
	public int Price { get; set; }

	[JsonProperty("priceRUB")]
	public int PriceRub { get; set; }

	[JsonProperty("currency")]
	public string Currency { get; set; } = string.Empty;

	[JsonProperty("currencyItem")]
	public string CurrencyItem { get; set; } = string.Empty;

	[JsonProperty("minTraderLevel")]
	public int? MinTraderLevel { get; set; }

	[JsonProperty("taskUnlock")]
	public string? TaskUnlock { get; set; }

	[JsonProperty("restockAmount")]
	public int? RestockAmount { get; set; }

	[JsonProperty("buyLimit")]
	public int? BuyLimit { get; set; }
}
