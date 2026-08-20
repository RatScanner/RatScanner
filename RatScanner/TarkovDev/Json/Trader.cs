using Newtonsoft.Json;
using System.Collections.Generic;
public class Levels {
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("requiredPlayerLevel")]
	public int RequiredPlayerLevel { get; set; }

	[JsonProperty("requiredReputation")]
	public int RequiredReputation { get; set; }

	[JsonProperty("requiredCommerce")]
	public int RequiredCommerce { get; set; }

	[JsonProperty("payRate")]
	public double PayRate { get; set; }

	[JsonProperty("insuranceRate")]
	public double InsuranceRate { get; set; }

	[JsonProperty("repairCostMultiplier")]
	public double RepairCostMultiplier { get; set; }
}

public class BuyAllowed {
	[JsonProperty("category")]
	public List<string> Category { get; set; }

	[JsonProperty("items")]
	public List<string> Items { get; set; }
}

public class BuyProhibited {
	[JsonProperty("category")]
	public List<object> Category { get; set; }

	[JsonProperty("items")]
	public List<string> Items { get; set; }
}

public class Trader {

	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("currency")]
	public string Currency { get; set; }

	[JsonProperty("resetTime")]
	public string ResetTime { get; set; }

	[JsonProperty("discount")]
	public int Discount { get; set; }

	[JsonProperty("levels")]
	public List<Levels> Levels { get; set; }

	[JsonProperty("reputationLevels")]
	public List<object> ReputationLevels { get; set; }

	[JsonProperty("imageLink")]
	public string ImageLink { get; set; }

	[JsonProperty("buyAllowed")]
	public BuyAllowed BuyAllowed { get; set; }

	[JsonProperty("buyProhibited")]
	public BuyProhibited BuyProhibited { get; set; }
}

public class BtrDeliveryGridSize {
	[JsonProperty("x")]
	public int X { get; set; }

	[JsonProperty("y")]
	public int Y { get; set; }

	[JsonProperty("z")]
	public int Z { get; set; }
}

public class ReputationLevels {
	[JsonProperty("minimumReputation")]
	public int MinimumReputation { get; set; }

	[JsonProperty("scavCooldownModifier")]
	public int ScavCooldownModifier { get; set; }

	[JsonProperty("scavCaseTimeModifier")]
	public int ScavCaseTimeModifier { get; set; }

	[JsonProperty("extractPriceModifier")]
	public int ExtractPriceModifier { get; set; }

	[JsonProperty("scavFollowChance")]
	public int ScavFollowChance { get; set; }

	[JsonProperty("scavEquipmentSpawnChanceModifier")]
	public int ScavEquipmentSpawnChanceModifier { get; set; }

	[JsonProperty("priceModifier")]
	public double PriceModifier { get; set; }

	[JsonProperty("hostileBosses")]
	public bool HostileBosses { get; set; }

	[JsonProperty("hostileScavs")]
	public bool HostileScavs { get; set; }

	[JsonProperty("scavAttackSupport")]
	public bool ScavAttackSupport { get; set; }

	[JsonProperty("availableScavExtracts")]
	public int AvailableScavExtracts { get; set; }

	[JsonProperty("btrEnabled")]
	public bool BtrEnabled { get; set; }

	[JsonProperty("btrDeliveryDiscount")]
	public int BtrDeliveryDiscount { get; set; }

	[JsonProperty("btrDeliveryGridSize")]
	public BtrDeliveryGridSize BtrDeliveryGridSize { get; set; }

	[JsonProperty("btrTaxiDiscount")]
	public int BtrTaxiDiscount { get; set; }

	[JsonProperty("btrCoveringFireDiscount")]
	public int BtrCoveringFireDiscount { get; set; }
}
