using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TraderLevel
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("requiredPlayerLevel")]
	public int RequiredPlayerLevel { get; set; }

	[JsonProperty("requiredReputation")]
	public double RequiredReputation { get; set; }

	[JsonProperty("requiredCommerce")]
	public int RequiredCommerce { get; set; }

	[JsonProperty("payRate")]
	public double PayRate { get; set; }

	[JsonProperty("insuranceRate")]
	public double? InsuranceRate { get; set; }

	[JsonProperty("repairCostMultiplier")]
	public double? RepairCostMultiplier { get; set; }

	[JsonProperty("barters")]
	public List<Barter> Barters { get; set; }

	[JsonProperty("cashOffers")]
	public List<TraderCashOffer> CashOffers { get; set; }
}
