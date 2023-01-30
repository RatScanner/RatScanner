using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class FleaMarket : IVendor
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("minPlayerLevel")]
	public int MinPlayerLevel { get; set; }

	[JsonProperty("enabled")]
	public bool Enabled { get; set; }

	[JsonProperty("sellOfferFeeRate")]
	public double SellOfferFeeRate { get; set; }

	[JsonProperty("sellRequirementFeeRate")]
	public double SellRequirementFeeRate { get; set; }

	[JsonProperty("reputationLevels")]
	public List<FleaMarketReputationLevel> ReputationLevels { get; set; }
}
