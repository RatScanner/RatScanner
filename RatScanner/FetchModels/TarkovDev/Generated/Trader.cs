using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Trader
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("resetTime")]
	public string ResetTime { get; set; }

	[JsonProperty("currency")]
	public Item Currency { get; set; }

	[JsonProperty("discount")]
	public double Discount { get; set; }

	[JsonProperty("levels")]
	public List<TraderLevel> Levels { get; set; }

	[JsonProperty("barters")]
	public List<Barter> Barters { get; set; }

	[JsonProperty("cashOffers")]
	public List<TraderCashOffer> CashOffers { get; set; }

	[JsonProperty("imageLink")]
	public string ImageLink { get; set; }

	[JsonProperty("image4xLink")]
	public string Image4xLink { get; set; }

	[JsonProperty("tarkovDataId")]
	public int? TarkovDataId { get; set; }
}
