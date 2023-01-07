using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RatScanner;

public class TarkovDevSchema
{

	#region Ammo
	public class Ammo
	{
		#region members
		[JsonProperty("item")]
		public Item item { get; set; }

		[JsonProperty("weight")]
		public double weight { get; set; }

		[JsonProperty("caliber")]
		public string caliber { get; set; }

		[JsonProperty("stackMaxSize")]
		public int stackMaxSize { get; set; }

		[JsonProperty("tracer")]
		public bool tracer { get; set; }

		[JsonProperty("tracerColor")]
		public string tracerColor { get; set; }

		[JsonProperty("ammoType")]
		public string ammoType { get; set; }

		[JsonProperty("projectileCount")]
		public int? projectileCount { get; set; }

		[JsonProperty("damage")]
		public int damage { get; set; }

		[JsonProperty("armorDamage")]
		public int armorDamage { get; set; }

		[JsonProperty("fragmentationChance")]
		public double fragmentationChance { get; set; }

		[JsonProperty("ricochetChance")]
		public double ricochetChance { get; set; }

		[JsonProperty("penetrationChance")]
		public double penetrationChance { get; set; }

		[JsonProperty("penetrationPower")]
		public int penetrationPower { get; set; }

		[Obsolete("Use accuracyModifier instead.")]
		[JsonProperty("accuracy")]
		public int? accuracy { get; set; }

		[JsonProperty("accuracyModifier")]
		public double? accuracyModifier { get; set; }

		[Obsolete("Use recoilModifier instead.")]
		[JsonProperty("recoil")]
		public int? recoil { get; set; }

		[JsonProperty("recoilModifier")]
		public double? recoilModifier { get; set; }

		[JsonProperty("initialSpeed")]
		public double? initialSpeed { get; set; }

		[JsonProperty("lightBleedModifier")]
		public double lightBleedModifier { get; set; }

		[JsonProperty("heavyBleedModifier")]
		public double heavyBleedModifier { get; set; }
		#endregion
	}
	#endregion

	#region ArmorMaterial
	public class ArmorMaterial
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("destructibility")]
		public double? destructibility { get; set; }

		[JsonProperty("minRepairDegradation")]
		public double? minRepairDegradation { get; set; }

		[JsonProperty("maxRepairDegradation")]
		public double? maxRepairDegradation { get; set; }

		[JsonProperty("explosionDestructibility")]
		public double? explosionDestructibility { get; set; }

		[JsonProperty("minRepairKitDegradation")]
		public double? minRepairKitDegradation { get; set; }

		[JsonProperty("maxRepairKitDegradation")]
		public double? maxRepairKitDegradation { get; set; }
		#endregion
	}
	#endregion

	#region AttributeThreshold
	public class AttributeThreshold
	{
		#region members
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("requirement")]
		public NumberCompare requirement { get; set; }
		#endregion
	}
	#endregion

	#region Barter
	public class Barter
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("trader")]
		public Trader trader { get; set; }

		[JsonProperty("level")]
		public int level { get; set; }

		[JsonProperty("taskUnlock")]
		public Task taskUnlock { get; set; }

		[JsonProperty("requiredItems")]
		public List<ContainedItem> requiredItems { get; set; }

		[JsonProperty("rewardItems")]
		public List<ContainedItem> rewardItems { get; set; }

		[Obsolete("Use trader and level instead.")]
		[JsonProperty("source")]
		public string source { get; set; }

		[Obsolete("Use trader instead.")]
		[JsonProperty("sourceName")]
		public ItemSourceName sourceName { get; set; }

		[Obsolete("Use level instead.")]
		[JsonProperty("requirements")]
		public List<PriceRequirement> requirements { get; set; }
		#endregion
	}
	#endregion

	#region BossSpawn
	public class BossSpawn
	{
		#region members
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("spawnChance")]
		public double spawnChance { get; set; }

		[JsonProperty("spawnLocations")]
		public List<BossSpawnLocation> spawnLocations { get; set; }

		[JsonProperty("escorts")]
		public List<BossEscort> escorts { get; set; }

		[JsonProperty("spawnTime")]
		public int? spawnTime { get; set; }

		[JsonProperty("spawnTimeRandom")]
		public bool? spawnTimeRandom { get; set; }

		[JsonProperty("spawnTrigger")]
		public string spawnTrigger { get; set; }
		#endregion
	}
	#endregion

	#region BossEscort
	public class BossEscort
	{
		#region members
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("amount")]
		public List<BossEscortAmount> amount { get; set; }
		#endregion
	}
	#endregion

	#region BossEscortAmount
	public class BossEscortAmount
	{
		#region members
		[JsonProperty("count")]
		public int count { get; set; }

		[JsonProperty("chance")]
		public double chance { get; set; }
		#endregion
	}
	#endregion

	#region BossSpawnLocation
	public class BossSpawnLocation
	{
		#region members
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("chance")]
		public double chance { get; set; }
		#endregion
	}
	#endregion

	#region ContainedItem
	public class ContainedItem
	{
		#region members
		[JsonProperty("item")]
		public Item item { get; set; }

		[JsonProperty("count")]
		public double count { get; set; }

		[JsonProperty("quantity")]
		public double quantity { get; set; }

		[JsonProperty("attributes")]
		public List<ItemAttribute> attributes { get; set; }
		#endregion
	}
	#endregion

	#region Craft
	public class Craft
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("station")]
		public HideoutStation station { get; set; }

		[JsonProperty("level")]
		public int level { get; set; }

		[JsonProperty("taskUnlock")]
		public Task taskUnlock { get; set; }

		[JsonProperty("duration")]
		public int duration { get; set; }

		[JsonProperty("requiredItems")]
		public List<ContainedItem> requiredItems { get; set; }

		[JsonProperty("requiredQuestItems")]
		public List<QuestItem> requiredQuestItems { get; set; }

		[JsonProperty("rewardItems")]
		public List<ContainedItem> rewardItems { get; set; }

		[Obsolete("Use stationLevel instead.")]
		[JsonProperty("source")]
		public string source { get; set; }

		[Obsolete("Use stationLevel instead.")]
		[JsonProperty("sourceName")]
		public string sourceName { get; set; }

		[Obsolete("Use stationLevel instead.")]
		[JsonProperty("requirements")]
		public List<PriceRequirement> requirements { get; set; }
		#endregion
	}
	#endregion

	#region GameProperty
	public class GameProperty
	{
		#region members
		[JsonProperty("key")]
		public string key { get; set; }

		[JsonProperty("numericValue")]
		public double? numericValue { get; set; }

		[JsonProperty("stringValue")]
		public string stringValue { get; set; }

		[JsonProperty("arrayValue")]
		public List<string> arrayValue { get; set; }

		[JsonProperty("objectValue")]
		public string objectValue { get; set; }
		#endregion
	}
	#endregion

	#region FleaMarket
	public class FleaMarket : Vendor
	{
		#region members
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("minPlayerLevel")]
		public int minPlayerLevel { get; set; }

		[JsonProperty("enabled")]
		public bool enabled { get; set; }

		[JsonProperty("sellOfferFeeRate")]
		public double sellOfferFeeRate { get; set; }

		[JsonProperty("sellRequirementFeeRate")]
		public double sellRequirementFeeRate { get; set; }

		[JsonProperty("reputationLevels")]
		public List<FleaMarketReputationLevel> reputationLevels { get; set; }
		#endregion
	}
	#endregion

	#region FleaMarketReputationLevel
	public class FleaMarketReputationLevel
	{
		#region members
		[JsonProperty("offers")]
		public int offers { get; set; }

		[JsonProperty("minRep")]
		public double minRep { get; set; }

		[JsonProperty("maxRep")]
		public double maxRep { get; set; }
		#endregion
	}
	#endregion

	#region HealthEffect
	public class HealthEffect
	{
		#region members
		[JsonProperty("bodyParts")]
		public List<string> bodyParts { get; set; }

		[JsonProperty("effects")]
		public List<string> effects { get; set; }

		[JsonProperty("time")]
		public NumberCompare time { get; set; }
		#endregion
	}
	#endregion

	#region HideoutStation
	public class HideoutStation
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("levels")]
		public List<HideoutStationLevel> levels { get; set; }

		[JsonProperty("tarkovDataId")]
		public int? tarkovDataId { get; set; }

		[JsonProperty("crafts")]
		public List<Craft> crafts { get; set; }
		#endregion
	}
	#endregion

	#region HideoutStationLevel
	public class HideoutStationLevel
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("level")]
		public int level { get; set; }

		[JsonProperty("constructionTime")]
		public int constructionTime { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("itemRequirements")]
		public List<RequirementItem> itemRequirements { get; set; }

		[JsonProperty("stationLevelRequirements")]
		public List<RequirementHideoutStationLevel> stationLevelRequirements { get; set; }

		[JsonProperty("skillRequirements")]
		public List<RequirementSkill> skillRequirements { get; set; }

		[JsonProperty("traderRequirements")]
		public List<RequirementTrader> traderRequirements { get; set; }

		[JsonProperty("tarkovDataId")]
		public int? tarkovDataId { get; set; }

		[JsonProperty("crafts")]
		public List<Craft> crafts { get; set; }
		#endregion
	}
	#endregion

	#region historicalPricePoint
	public class historicalPricePoint
	{
		#region members
		[JsonProperty("price")]
		public int? price { get; set; }

		[JsonProperty("timestamp")]
		public string timestamp { get; set; }
		#endregion
	}
	#endregion

	#region Item
	public class Item
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("shortName")]
		public string shortName { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("basePrice")]
		public int basePrice { get; set; }

		[JsonProperty("updated")]
		public string updated { get; set; }

		[JsonProperty("width")]
		public int width { get; set; }

		[JsonProperty("height")]
		public int height { get; set; }

		[JsonProperty("backgroundColor")]
		public string backgroundColor { get; set; }

		[JsonProperty("iconLink")]
		public string iconLink { get; set; }

		[JsonProperty("gridImageLink")]
		public string gridImageLink { get; set; }

		[JsonProperty("baseImageLink")]
		public string baseImageLink { get; set; }

		[JsonProperty("inspectImageLink")]
		public string inspectImageLink { get; set; }

		[JsonProperty("image512pxLink")]
		public string image512pxLink { get; set; }

		[JsonProperty("image8xLink")]
		public string image8xLink { get; set; }

		[JsonProperty("wikiLink")]
		public string wikiLink { get; set; }

		[JsonProperty("types")]
		public List<ItemType?> types { get; set; }

		[JsonProperty("avg24hPrice")]
		public int? avg24hPrice { get; set; }

		[JsonProperty("properties")]
		public ItemProperties properties { get; set; }

		[JsonProperty("conflictingItems")]
		public List<Item> conflictingItems { get; set; }

		[JsonProperty("conflictingSlotIds")]
		public List<string> conflictingSlotIds { get; set; }

		[JsonProperty("accuracyModifier")]
		public double? accuracyModifier { get; set; }

		[JsonProperty("recoilModifier")]
		public double? recoilModifier { get; set; }

		[JsonProperty("ergonomicsModifier")]
		public double? ergonomicsModifier { get; set; }

		[JsonProperty("hasGrid")]
		public bool? hasGrid { get; set; }

		[JsonProperty("blocksHeadphones")]
		public bool? blocksHeadphones { get; set; }

		[JsonProperty("link")]
		public string link { get; set; }

		[JsonProperty("lastLowPrice")]
		public int? lastLowPrice { get; set; }

		[JsonProperty("changeLast48h")]
		public double? changeLast48h { get; set; }

		[JsonProperty("changeLast48hPercent")]
		public double? changeLast48hPercent { get; set; }

		[JsonProperty("low24hPrice")]
		public int? low24hPrice { get; set; }

		[JsonProperty("high24hPrice")]
		public int? high24hPrice { get; set; }

		[JsonProperty("lastOfferCount")]
		public int? lastOfferCount { get; set; }

		[JsonProperty("sellFor")]
		public List<ItemPrice> sellFor { get; set; }

		[JsonProperty("buyFor")]
		public List<ItemPrice> buyFor { get; set; }

		[JsonProperty("containsItems")]
		public List<ContainedItem> containsItems { get; set; }

		[JsonProperty("category")]
		public ItemCategory category { get; set; }

		[JsonProperty("categories")]
		public List<ItemCategory> categories { get; set; }

		[JsonProperty("bsgCategoryId")]
		public string bsgCategoryId { get; set; }

		[JsonProperty("handbookCategories")]
		public List<ItemCategory> handbookCategories { get; set; }

		[JsonProperty("weight")]
		public double? weight { get; set; }

		[JsonProperty("velocity")]
		public double? velocity { get; set; }

		[JsonProperty("loudness")]
		public int? loudness { get; set; }

		[JsonProperty("usedInTasks")]
		public List<Task> usedInTasks { get; set; }

		[JsonProperty("receivedFromTasks")]
		public List<Task> receivedFromTasks { get; set; }

		[JsonProperty("bartersFor")]
		public List<Barter> bartersFor { get; set; }

		[JsonProperty("bartersUsing")]
		public List<Barter> bartersUsing { get; set; }

		[JsonProperty("craftsFor")]
		public List<Craft> craftsFor { get; set; }

		[JsonProperty("craftsUsing")]
		public List<Craft> craftsUsing { get; set; }

		[JsonProperty("historicalPrices")]
		public List<historicalPricePoint> historicalPrices { get; set; }

		[JsonProperty("fleaMarketFee")]
		public int? fleaMarketFee { get; set; }

		[Obsolete("No longer meaningful with inclusion of Item category.")]
		[JsonProperty("categoryTop")]
		public ItemCategory categoryTop { get; set; }

		[Obsolete("Use the lang argument on queries instead.")]
		[JsonProperty("translation")]
		public ItemTranslation translation { get; set; }

		[Obsolete("Use sellFor instead.")]
		[JsonProperty("traderPrices")]
		public List<TraderPrice> traderPrices { get; set; }

		[Obsolete("Use category instead.")]
		[JsonProperty("bsgCategory")]
		public ItemCategory bsgCategory { get; set; }

		[Obsolete("Use inspectImageLink instead.")]
		[JsonProperty("imageLink")]
		public string imageLink { get; set; }

		[Obsolete("Fallback handled automatically by inspectImageLink.")]
		[JsonProperty("imageLinkFallback")]
		public string imageLinkFallback { get; set; }

		[Obsolete("Fallback handled automatically by iconLink.")]
		[JsonProperty("iconLinkFallback")]
		public string iconLinkFallback { get; set; }

		[Obsolete("Fallback handled automatically by gridImageLink.")]
		[JsonProperty("gridImageLinkFallback")]
		public string gridImageLinkFallback { get; set; }
		#endregion
	}
	#endregion

	#region ItemAttribute
	public class ItemAttribute
	{
		#region members
		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("value")]
		public string value { get; set; }
		#endregion
	}
	#endregion

	#region ItemCategory
	public class ItemCategory
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("parent")]
		public ItemCategory parent { get; set; }

		[JsonProperty("children")]
		public List<ItemCategory> children { get; set; }
		#endregion
	}
	#endregion

	#region ItemFilters
	public class ItemFilters
	{
		#region members
		[JsonProperty("allowedCategories")]
		public List<ItemCategory> allowedCategories { get; set; }

		[JsonProperty("allowedItems")]
		public List<Item> allowedItems { get; set; }

		[JsonProperty("excludedCategories")]
		public List<ItemCategory> excludedCategories { get; set; }

		[JsonProperty("excludedItems")]
		public List<Item> excludedItems { get; set; }
		#endregion
	}
	#endregion

	#region ItemPrice
	public class ItemPrice
	{
		#region members
		[JsonProperty("vendor")]
		public Vendor vendor { get; set; }

		[JsonProperty("price")]
		public int? price { get; set; }

		[JsonProperty("currency")]
		public string currency { get; set; }

		[JsonProperty("currencyItem")]
		public Item currencyItem { get; set; }

		[JsonProperty("priceRUB")]
		public int? priceRUB { get; set; }

		[Obsolete("Use vendor instead.")]
		[JsonProperty("source")]
		public ItemSourceName? source { get; set; }

		[Obsolete("Use vendor instead.")]
		[JsonProperty("requirements")]
		public List<PriceRequirement> requirements { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesAmmo
	public class ItemPropertiesAmmo
	{
		#region members
		[JsonProperty("caliber")]
		public string caliber { get; set; }

		[JsonProperty("stackMaxSize")]
		public int? stackMaxSize { get; set; }

		[JsonProperty("tracer")]
		public bool? tracer { get; set; }

		[JsonProperty("tracerColor")]
		public string tracerColor { get; set; }

		[JsonProperty("ammoType")]
		public string ammoType { get; set; }

		[JsonProperty("projectileCount")]
		public int? projectileCount { get; set; }

		[JsonProperty("damage")]
		public int? damage { get; set; }

		[JsonProperty("armorDamage")]
		public int? armorDamage { get; set; }

		[JsonProperty("fragmentationChance")]
		public double? fragmentationChance { get; set; }

		[JsonProperty("ricochetChance")]
		public double? ricochetChance { get; set; }

		[JsonProperty("penetrationChance")]
		public double? penetrationChance { get; set; }

		[JsonProperty("penetrationPower")]
		public int? penetrationPower { get; set; }

		[Obsolete("Use accuracyModifier instead.")]
		[JsonProperty("accuracy")]
		public int? accuracy { get; set; }

		[JsonProperty("accuracyModifier")]
		public double? accuracyModifier { get; set; }

		[Obsolete("Use recoilModifier instead.")]
		[JsonProperty("recoil")]
		public double? recoil { get; set; }

		[JsonProperty("recoilModifier")]
		public double? recoilModifier { get; set; }

		[JsonProperty("initialSpeed")]
		public double? initialSpeed { get; set; }

		[JsonProperty("lightBleedModifier")]
		public double? lightBleedModifier { get; set; }

		[JsonProperty("heavyBleedModifier")]
		public double? heavyBleedModifier { get; set; }

		[JsonProperty("durabilityBurnFactor")]
		public double? durabilityBurnFactor { get; set; }

		[JsonProperty("heatFactor")]
		public double? heatFactor { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesArmor
	public class ItemPropertiesArmor
	{
		#region members
		[JsonProperty("class")]
		public int? @class { get; set; }

		[JsonProperty("durability")]
		public int? durability { get; set; }

		[JsonProperty("repairCost")]
		public int? repairCost { get; set; }

		[JsonProperty("speedPenalty")]
		public double? speedPenalty { get; set; }

		[JsonProperty("turnPenalty")]
		public double? turnPenalty { get; set; }

		[JsonProperty("ergoPenalty")]
		public int? ergoPenalty { get; set; }

		[JsonProperty("zones")]
		public List<string> zones { get; set; }

		[JsonProperty("material")]
		public ArmorMaterial material { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesArmorAttachment
	public class ItemPropertiesArmorAttachment
	{
		#region members
		[JsonProperty("class")]
		public int? @class { get; set; }

		[JsonProperty("durability")]
		public int? durability { get; set; }

		[JsonProperty("repairCost")]
		public int? repairCost { get; set; }

		[JsonProperty("speedPenalty")]
		public double? speedPenalty { get; set; }

		[JsonProperty("turnPenalty")]
		public double? turnPenalty { get; set; }

		[JsonProperty("ergoPenalty")]
		public int? ergoPenalty { get; set; }

		[JsonProperty("headZones")]
		public List<string> headZones { get; set; }

		[JsonProperty("material")]
		public ArmorMaterial material { get; set; }

		[JsonProperty("blindnessProtection")]
		public double? blindnessProtection { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesBackpack
	public class ItemPropertiesBackpack
	{
		#region members
		[JsonProperty("capacity")]
		public int? capacity { get; set; }

		[JsonProperty("grids")]
		public List<ItemStorageGrid> grids { get; set; }

		[Obsolete("Use grids instead.")]
		[JsonProperty("pouches")]
		public List<ItemStorageGrid> pouches { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesBarrel
	public class ItemPropertiesBarrel
	{
		#region members
		[JsonProperty("ergonomics")]
		public double? ergonomics { get; set; }

		[Obsolete("Use recoilModifier instead.")]
		[JsonProperty("recoil")]
		public double? recoil { get; set; }

		[JsonProperty("recoilModifier")]
		public double? recoilModifier { get; set; }

		[Obsolete("Use centerOfImpact, deviationCurve, and deviationMax instead.")]
		[JsonProperty("accuracyModifier")]
		public double? accuracyModifier { get; set; }

		[JsonProperty("centerOfImpact")]
		public double? centerOfImpact { get; set; }

		[JsonProperty("deviationCurve")]
		public double? deviationCurve { get; set; }

		[JsonProperty("deviationMax")]
		public double? deviationMax { get; set; }

		[JsonProperty("slots")]
		public List<ItemSlot> slots { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesChestRig
	public class ItemPropertiesChestRig
	{
		#region members
		[JsonProperty("class")]
		public int? @class { get; set; }

		[JsonProperty("durability")]
		public int? durability { get; set; }

		[JsonProperty("repairCost")]
		public int? repairCost { get; set; }

		[JsonProperty("speedPenalty")]
		public double? speedPenalty { get; set; }

		[JsonProperty("turnPenalty")]
		public double? turnPenalty { get; set; }

		[JsonProperty("ergoPenalty")]
		public int? ergoPenalty { get; set; }

		[JsonProperty("zones")]
		public List<string> zones { get; set; }

		[JsonProperty("material")]
		public ArmorMaterial material { get; set; }

		[JsonProperty("capacity")]
		public int? capacity { get; set; }

		[JsonProperty("grids")]
		public List<ItemStorageGrid> grids { get; set; }

		[Obsolete("Use grids instead.")]
		[JsonProperty("pouches")]
		public List<ItemStorageGrid> pouches { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesContainer
	public class ItemPropertiesContainer
	{
		#region members
		[JsonProperty("capacity")]
		public int? capacity { get; set; }

		[JsonProperty("grids")]
		public List<ItemStorageGrid> grids { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesFoodDrink
	public class ItemPropertiesFoodDrink
	{
		#region members
		[JsonProperty("energy")]
		public int? energy { get; set; }

		[JsonProperty("hydration")]
		public int? hydration { get; set; }

		[JsonProperty("units")]
		public int? units { get; set; }

		[JsonProperty("stimEffects")]
		public List<StimEffect> stimEffects { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesGlasses
	public class ItemPropertiesGlasses
	{
		#region members
		[JsonProperty("class")]
		public int? @class { get; set; }

		[JsonProperty("durability")]
		public int? durability { get; set; }

		[JsonProperty("repairCost")]
		public int? repairCost { get; set; }

		[JsonProperty("blindnessProtection")]
		public double? blindnessProtection { get; set; }

		[JsonProperty("material")]
		public ArmorMaterial material { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesGrenade
	public class ItemPropertiesGrenade
	{
		#region members
		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("fuse")]
		public double? fuse { get; set; }

		[JsonProperty("minExplosionDistance")]
		public int? minExplosionDistance { get; set; }

		[JsonProperty("maxExplosionDistance")]
		public int? maxExplosionDistance { get; set; }

		[JsonProperty("fragments")]
		public int? fragments { get; set; }

		[JsonProperty("contusionRadius")]
		public int? contusionRadius { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesHelmet
	public class ItemPropertiesHelmet
	{
		#region members
		[JsonProperty("class")]
		public int? @class { get; set; }

		[JsonProperty("durability")]
		public int? durability { get; set; }

		[JsonProperty("repairCost")]
		public int? repairCost { get; set; }

		[JsonProperty("speedPenalty")]
		public double? speedPenalty { get; set; }

		[JsonProperty("turnPenalty")]
		public double? turnPenalty { get; set; }

		[JsonProperty("ergoPenalty")]
		public int? ergoPenalty { get; set; }

		[JsonProperty("headZones")]
		public List<string> headZones { get; set; }

		[JsonProperty("material")]
		public ArmorMaterial material { get; set; }

		[JsonProperty("deafening")]
		public string deafening { get; set; }

		[JsonProperty("blocksHeadset")]
		public bool? blocksHeadset { get; set; }

		[JsonProperty("blindnessProtection")]
		public double? blindnessProtection { get; set; }

		[JsonProperty("slots")]
		public List<ItemSlot> slots { get; set; }

		[JsonProperty("ricochetX")]
		public double? ricochetX { get; set; }

		[JsonProperty("ricochetY")]
		public double? ricochetY { get; set; }

		[JsonProperty("ricochetZ")]
		public double? ricochetZ { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesKey
	public class ItemPropertiesKey
	{
		#region members
		[JsonProperty("uses")]
		public int? uses { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesMagazine
	public class ItemPropertiesMagazine
	{
		#region members
		[JsonProperty("ergonomics")]
		public double? ergonomics { get; set; }

		[Obsolete("Use recoilModifier instead.")]
		[JsonProperty("recoil")]
		public double? recoil { get; set; }

		[JsonProperty("recoilModifier")]
		public double? recoilModifier { get; set; }

		[JsonProperty("capacity")]
		public int? capacity { get; set; }

		[JsonProperty("loadModifier")]
		public double? loadModifier { get; set; }

		[JsonProperty("ammoCheckModifier")]
		public double? ammoCheckModifier { get; set; }

		[JsonProperty("malfunctionChance")]
		public double? malfunctionChance { get; set; }

		[JsonProperty("slots")]
		public List<ItemSlot> slots { get; set; }

		[JsonProperty("allowedAmmo")]
		public List<Item> allowedAmmo { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesMedicalItem
	public class ItemPropertiesMedicalItem
	{
		#region members
		[JsonProperty("uses")]
		public int? uses { get; set; }

		[JsonProperty("useTime")]
		public int? useTime { get; set; }

		[JsonProperty("cures")]
		public List<string> cures { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesMedKit
	public class ItemPropertiesMedKit
	{
		#region members
		[JsonProperty("hitpoints")]
		public int? hitpoints { get; set; }

		[JsonProperty("useTime")]
		public int? useTime { get; set; }

		[JsonProperty("maxHealPerUse")]
		public int? maxHealPerUse { get; set; }

		[JsonProperty("cures")]
		public List<string> cures { get; set; }

		[JsonProperty("hpCostLightBleeding")]
		public int? hpCostLightBleeding { get; set; }

		[JsonProperty("hpCostHeavyBleeding")]
		public int? hpCostHeavyBleeding { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesMelee
	public class ItemPropertiesMelee
	{
		#region members
		[JsonProperty("slashDamage")]
		public int? slashDamage { get; set; }

		[JsonProperty("stabDamage")]
		public int? stabDamage { get; set; }

		[JsonProperty("hitRadius")]
		public double? hitRadius { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesNightVision
	public class ItemPropertiesNightVision
	{
		#region members
		[JsonProperty("intensity")]
		public double? intensity { get; set; }

		[JsonProperty("noiseIntensity")]
		public double? noiseIntensity { get; set; }

		[JsonProperty("noiseScale")]
		public double? noiseScale { get; set; }

		[JsonProperty("diffuseIntensity")]
		public double? diffuseIntensity { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesPainkiller
	public class ItemPropertiesPainkiller
	{
		#region members
		[JsonProperty("uses")]
		public int? uses { get; set; }

		[JsonProperty("useTime")]
		public int? useTime { get; set; }

		[JsonProperty("cures")]
		public List<string> cures { get; set; }

		[JsonProperty("painkillerDuration")]
		public int? painkillerDuration { get; set; }

		[JsonProperty("energyImpact")]
		public int? energyImpact { get; set; }

		[JsonProperty("hydrationImpact")]
		public int? hydrationImpact { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesPreset
	public class ItemPropertiesPreset
	{
		#region members
		[JsonProperty("baseItem")]
		public Item baseItem { get; set; }

		[JsonProperty("ergonomics")]
		public double? ergonomics { get; set; }

		[JsonProperty("recoilVertical")]
		public int? recoilVertical { get; set; }

		[JsonProperty("recoilHorizontal")]
		public int? recoilHorizontal { get; set; }

		[JsonProperty("moa")]
		public double? moa { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesScope
	public class ItemPropertiesScope
	{
		#region members
		[JsonProperty("ergonomics")]
		public double? ergonomics { get; set; }

		[JsonProperty("sightModes")]
		public List<int?> sightModes { get; set; }

		[Obsolete("Use recoilModifier instead.")]
		[JsonProperty("recoil")]
		public double? recoil { get; set; }

		[JsonProperty("sightingRange")]
		public int? sightingRange { get; set; }

		[JsonProperty("recoilModifier")]
		public double? recoilModifier { get; set; }

		[JsonProperty("slots")]
		public List<ItemSlot> slots { get; set; }

		[JsonProperty("zoomLevels")]
		public List<List<double?>> zoomLevels { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesStim
	public class ItemPropertiesStim
	{
		#region members
		[JsonProperty("useTime")]
		public int? useTime { get; set; }

		[JsonProperty("cures")]
		public List<string> cures { get; set; }

		[JsonProperty("stimEffects")]
		public List<StimEffect> stimEffects { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesSurgicalKit
	public class ItemPropertiesSurgicalKit
	{
		#region members
		[JsonProperty("uses")]
		public int? uses { get; set; }

		[JsonProperty("useTime")]
		public int? useTime { get; set; }

		[JsonProperty("cures")]
		public List<string> cures { get; set; }

		[JsonProperty("minLimbHealth")]
		public double? minLimbHealth { get; set; }

		[JsonProperty("maxLimbHealth")]
		public double? maxLimbHealth { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesWeapon
	public class ItemPropertiesWeapon
	{
		#region members
		[JsonProperty("caliber")]
		public string caliber { get; set; }

		[JsonProperty("defaultAmmo")]
		public Item defaultAmmo { get; set; }

		[JsonProperty("effectiveDistance")]
		public int? effectiveDistance { get; set; }

		[JsonProperty("ergonomics")]
		public double? ergonomics { get; set; }

		[JsonProperty("fireModes")]
		public List<string> fireModes { get; set; }

		[JsonProperty("fireRate")]
		public int? fireRate { get; set; }

		[JsonProperty("maxDurability")]
		public int? maxDurability { get; set; }

		[JsonProperty("recoilVertical")]
		public int? recoilVertical { get; set; }

		[JsonProperty("recoilHorizontal")]
		public int? recoilHorizontal { get; set; }

		[JsonProperty("repairCost")]
		public int? repairCost { get; set; }

		[JsonProperty("sightingRange")]
		public int? sightingRange { get; set; }

		[JsonProperty("centerOfImpact")]
		public double? centerOfImpact { get; set; }

		[JsonProperty("deviationCurve")]
		public double? deviationCurve { get; set; }

		[JsonProperty("deviationMax")]
		public double? deviationMax { get; set; }

		[JsonProperty("defaultWidth")]
		public int? defaultWidth { get; set; }

		[JsonProperty("defaultHeight")]
		public int? defaultHeight { get; set; }

		[JsonProperty("defaultErgonomics")]
		public double? defaultErgonomics { get; set; }

		[JsonProperty("defaultRecoilVertical")]
		public int? defaultRecoilVertical { get; set; }

		[JsonProperty("defaultRecoilHorizontal")]
		public int? defaultRecoilHorizontal { get; set; }

		[JsonProperty("defaultWeight")]
		public double? defaultWeight { get; set; }

		[JsonProperty("defaultPreset")]
		public Item defaultPreset { get; set; }

		[JsonProperty("presets")]
		public List<Item> presets { get; set; }

		[JsonProperty("slots")]
		public List<ItemSlot> slots { get; set; }

		[JsonProperty("allowedAmmo")]
		public List<Item> allowedAmmo { get; set; }
		#endregion
	}
	#endregion

	#region ItemPropertiesWeaponMod
	public class ItemPropertiesWeaponMod
	{
		#region members
		[JsonProperty("ergonomics")]
		public double? ergonomics { get; set; }

		[Obsolete("Use recoilModifier instead.")]
		[JsonProperty("recoil")]
		public double? recoil { get; set; }

		[JsonProperty("recoilModifier")]
		public double? recoilModifier { get; set; }

		[JsonProperty("accuracyModifier")]
		public double? accuracyModifier { get; set; }

		[JsonProperty("slots")]
		public List<ItemSlot> slots { get; set; }
		#endregion
	}
	#endregion

	#region ItemSlot
	public class ItemSlot
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("nameId")]
		public string nameId { get; set; }

		[JsonProperty("filters")]
		public ItemFilters filters { get; set; }

		[JsonProperty("required")]
		public bool? required { get; set; }
		#endregion
	}
	#endregion
	public enum ItemSourceName
	{
		prapor,
		therapist,
		fence,
		skier,
		peacekeeper,
		mechanic,
		ragman,
		jaeger,
		fleaMarket
	}


	#region ItemStorageGrid
	public class ItemStorageGrid
	{
		#region members
		[JsonProperty("width")]
		public int width { get; set; }

		[JsonProperty("height")]
		public int height { get; set; }

		[JsonProperty("filters")]
		public ItemFilters filters { get; set; }
		#endregion
	}
	#endregion

	#region Map
	public class Map
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("tarkovDataId")]
		public string tarkovDataId { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("wiki")]
		public string wiki { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("enemies")]
		public List<string> enemies { get; set; }

		[JsonProperty("raidDuration")]
		public int? raidDuration { get; set; }

		[JsonProperty("players")]
		public string players { get; set; }

		[JsonProperty("bosses")]
		public List<BossSpawn> bosses { get; set; }

		[JsonProperty("nameId")]
		public string nameId { get; set; }
		#endregion
	}
	#endregion

	#region NumberCompare
	public class NumberCompare
	{
		#region members
		[JsonProperty("compareMethod")]
		public string compareMethod { get; set; }

		[JsonProperty("value")]
		public double value { get; set; }
		#endregion
	}
	#endregion

	#region OfferUnlock
	public class OfferUnlock
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("trader")]
		public Trader trader { get; set; }

		[JsonProperty("level")]
		public int level { get; set; }

		[JsonProperty("item")]
		public Item item { get; set; }
		#endregion
	}
	#endregion

	#region PlayerLevel
	public class PlayerLevel
	{
		#region members
		[JsonProperty("level")]
		public int level { get; set; }

		[JsonProperty("exp")]
		public int exp { get; set; }
		#endregion
	}
	#endregion

	#region PriceRequirement
	public class PriceRequirement
	{
		#region members
		[JsonProperty("type")]
		public RequirementType type { get; set; }

		[JsonProperty("value")]
		public int? value { get; set; }

		[JsonProperty("stringValue")]
		public string stringValue { get; set; }
		#endregion
	}
	#endregion

	#region QuestItem
	public class QuestItem
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("shortName")]
		public string shortName { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("width")]
		public int? width { get; set; }

		[JsonProperty("height")]
		public int? height { get; set; }

		[JsonProperty("iconLink")]
		public string iconLink { get; set; }

		[JsonProperty("gridImageLink")]
		public string gridImageLink { get; set; }

		[JsonProperty("baseImageLink")]
		public string baseImageLink { get; set; }

		[JsonProperty("inspectImageLink")]
		public string inspectImageLink { get; set; }

		[JsonProperty("image512pxLink")]
		public string image512pxLink { get; set; }

		[JsonProperty("image8xLink")]
		public string image8xLink { get; set; }
		#endregion
	}
	#endregion

	#region RequirementHideoutStationLevel
	public class RequirementHideoutStationLevel
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("station")]
		public HideoutStation station { get; set; }

		[JsonProperty("level")]
		public int level { get; set; }
		#endregion
	}
	#endregion

	#region RequirementItem
	public class RequirementItem
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("item")]
		public Item item { get; set; }

		[JsonProperty("count")]
		public int count { get; set; }

		[JsonProperty("quantity")]
		public int quantity { get; set; }

		[JsonProperty("attributes")]
		public List<ItemAttribute> attributes { get; set; }
		#endregion
	}
	#endregion

	#region RequirementSkill
	public class RequirementSkill
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("level")]
		public int level { get; set; }
		#endregion
	}
	#endregion

	#region RequirementTask
	public class RequirementTask
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("task")]
		public Task task { get; set; }
		#endregion
	}
	#endregion

	#region RequirementTrader
	public class RequirementTrader
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("trader")]
		public Trader trader { get; set; }

		[JsonProperty("level")]
		public int level { get; set; }
		#endregion
	}
	#endregion
	public enum RequirementType
	{
		playerLevel,
		loyaltyLevel,
		questCompleted,
		stationLevel
	}


	#region ServerStatus
	public class ServerStatus
	{
		#region members
		[JsonProperty("generalStatus")]
		public Status generalStatus { get; set; }

		[JsonProperty("currentStatuses")]
		public List<Status> currentStatuses { get; set; }

		[JsonProperty("messages")]
		public List<StatusMessage> messages { get; set; }
		#endregion
	}
	#endregion

	#region SkillLevel
	public class SkillLevel
	{
		#region members
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("level")]
		public double level { get; set; }
		#endregion
	}
	#endregion

	#region Status
	public class Status
	{
		#region members
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("message")]
		public string message { get; set; }

		[JsonProperty("status")]
		public int status { get; set; }

		[JsonProperty("statusCode")]
		public string statusCode { get; set; }
		#endregion
	}
	#endregion
	public enum StatusCode
	{
		OK,
		Updating,
		Unstable,
		Down
	}


	#region StatusMessage
	public class StatusMessage
	{
		#region members
		[JsonProperty("content")]
		public string content { get; set; }

		[JsonProperty("time")]
		public string time { get; set; }

		[JsonProperty("type")]
		public int type { get; set; }

		[JsonProperty("solveTime")]
		public string solveTime { get; set; }

		[JsonProperty("statusCode")]
		public string statusCode { get; set; }
		#endregion
	}
	#endregion

	#region StimEffect
	public class StimEffect
	{
		#region members
		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("chance")]
		public double chance { get; set; }

		[JsonProperty("delay")]
		public int delay { get; set; }

		[JsonProperty("duration")]
		public int duration { get; set; }

		[JsonProperty("value")]
		public double value { get; set; }

		[JsonProperty("percent")]
		public bool percent { get; set; }

		[JsonProperty("skillName")]
		public string skillName { get; set; }
		#endregion
	}
	#endregion

	#region Task
	public class Task
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("tarkovDataId")]
		public int? tarkovDataId { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("trader")]
		public Trader trader { get; set; }

		[JsonProperty("map")]
		public Map map { get; set; }

		[JsonProperty("experience")]
		public int experience { get; set; }

		[JsonProperty("wikiLink")]
		public string wikiLink { get; set; }

		[JsonProperty("minPlayerLevel")]
		public int? minPlayerLevel { get; set; }

		[JsonProperty("taskRequirements")]
		public List<TaskStatusRequirement> taskRequirements { get; set; }

		[JsonProperty("traderLevelRequirements")]
		public List<RequirementTrader> traderLevelRequirements { get; set; }

		[JsonProperty("objectives")]
		public List<TaskObjective> objectives { get; set; }

		[JsonProperty("startRewards")]
		public TaskRewards startRewards { get; set; }

		[JsonProperty("finishRewards")]
		public TaskRewards finishRewards { get; set; }

		[JsonProperty("factionName")]
		public string factionName { get; set; }

		[JsonProperty("neededKeys")]
		public List<TaskKey> neededKeys { get; set; }

		[JsonProperty("descriptionMessageId")]
		public string descriptionMessageId { get; set; }

		[JsonProperty("startMessageId")]
		public string startMessageId { get; set; }

		[JsonProperty("successMessageId")]
		public string successMessageId { get; set; }

		[JsonProperty("failMessageId")]
		public string failMessageId { get; set; }
		#endregion
	}
	#endregion

	#region TaskKey
	public class TaskKey
	{
		#region members
		[JsonProperty("keys")]
		public List<Item> keys { get; set; }

		[JsonProperty("map")]
		public Map map { get; set; }
		#endregion
	}
	#endregion

	public interface TaskObjective
	{
		[JsonProperty("id")]
		string id { get; set; }

		[JsonProperty("type")]
		string type { get; set; }

		[JsonProperty("description")]
		string description { get; set; }

		[JsonProperty("maps")]
		List<Map> maps { get; set; }

		[JsonProperty("optional")]
		bool optional { get; set; }
	}

	#region TaskObjectiveBasic
	public class TaskObjectiveBasic : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveBuildItem
	public class TaskObjectiveBuildItem : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("item")]
		public Item item { get; set; }

		[JsonProperty("containsAll")]
		public List<Item> containsAll { get; set; }

		[JsonProperty("containsOne")]
		public List<Item> containsOne { get; set; }

		[JsonProperty("attributes")]
		public List<AttributeThreshold> attributes { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveExperience
	public class TaskObjectiveExperience : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("healthEffect")]
		public HealthEffect healthEffect { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveExtract
	public class TaskObjectiveExtract : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("exitStatus")]
		public List<string> exitStatus { get; set; }

		[JsonProperty("exitName")]
		public string exitName { get; set; }

		[JsonProperty("zoneNames")]
		public List<string> zoneNames { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveItem
	public class TaskObjectiveItem : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("item")]
		public Item item { get; set; }

		[JsonProperty("count")]
		public int count { get; set; }

		[JsonProperty("foundInRaid")]
		public bool foundInRaid { get; set; }

		[JsonProperty("dogTagLevel")]
		public int? dogTagLevel { get; set; }

		[JsonProperty("maxDurability")]
		public int? maxDurability { get; set; }

		[JsonProperty("minDurability")]
		public int? minDurability { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveMark
	public class TaskObjectiveMark : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("markerItem")]
		public Item markerItem { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectivePlayerLevel
	public class TaskObjectivePlayerLevel : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("playerLevel")]
		public int playerLevel { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveQuestItem
	public class TaskObjectiveQuestItem : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("questItem")]
		public QuestItem questItem { get; set; }

		[JsonProperty("count")]
		public int count { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveShoot
	public class TaskObjectiveShoot : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("target")]
		public string target { get; set; }

		[JsonProperty("count")]
		public int count { get; set; }

		[JsonProperty("shotType")]
		public string shotType { get; set; }

		[JsonProperty("zoneNames")]
		public List<string> zoneNames { get; set; }

		[JsonProperty("bodyParts")]
		public List<string> bodyParts { get; set; }

		[JsonProperty("usingWeapon")]
		public List<Item> usingWeapon { get; set; }

		[JsonProperty("usingWeaponMods")]
		public List<List<Item>> usingWeaponMods { get; set; }

		[JsonProperty("wearing")]
		public List<List<Item>> wearing { get; set; }

		[JsonProperty("notWearing")]
		public List<Item> notWearing { get; set; }

		[JsonProperty("distance")]
		public NumberCompare distance { get; set; }

		[JsonProperty("playerHealthEffect")]
		public HealthEffect playerHealthEffect { get; set; }

		[JsonProperty("enemyHealthEffect")]
		public HealthEffect enemyHealthEffect { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveSkill
	public class TaskObjectiveSkill : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("skillLevel")]
		public SkillLevel skillLevel { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveTaskStatus
	public class TaskObjectiveTaskStatus : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("task")]
		public Task task { get; set; }

		[JsonProperty("status")]
		public List<string> status { get; set; }
		#endregion
	}
	#endregion

	#region TaskObjectiveTraderLevel
	public class TaskObjectiveTraderLevel : TaskObjective
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("optional")]
		public bool optional { get; set; }

		[JsonProperty("trader")]
		public Trader trader { get; set; }

		[JsonProperty("level")]
		public int level { get; set; }
		#endregion
	}
	#endregion

	#region TaskRewards
	public class TaskRewards
	{
		#region members
		[JsonProperty("traderStanding")]
		public List<TraderStanding> traderStanding { get; set; }

		[JsonProperty("items")]
		public List<ContainedItem> items { get; set; }

		[JsonProperty("offerUnlock")]
		public List<OfferUnlock> offerUnlock { get; set; }

		[JsonProperty("skillLevelReward")]
		public List<SkillLevel> skillLevelReward { get; set; }

		[JsonProperty("traderUnlock")]
		public List<Trader> traderUnlock { get; set; }

		[JsonProperty("craftUnlock")]
		public List<Craft> craftUnlock { get; set; }
		#endregion
	}
	#endregion

	#region TaskStatusRequirement
	public class TaskStatusRequirement
	{
		#region members
		[JsonProperty("task")]
		public Task task { get; set; }

		[JsonProperty("status")]
		public List<string> status { get; set; }
		#endregion
	}
	#endregion

	#region Trader
	public class Trader
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("description")]
		public string description { get; set; }

		[JsonProperty("resetTime")]
		public string resetTime { get; set; }

		[JsonProperty("currency")]
		public Item currency { get; set; }

		[JsonProperty("discount")]
		public double discount { get; set; }

		[JsonProperty("levels")]
		public List<TraderLevel> levels { get; set; }

		[JsonProperty("barters")]
		public List<Barter> barters { get; set; }

		[JsonProperty("cashOffers")]
		public List<TraderCashOffer> cashOffers { get; set; }

		[JsonProperty("tarkovDataId")]
		public int? tarkovDataId { get; set; }
		#endregion
	}
	#endregion

	#region TraderLevel
	public class TraderLevel
	{
		#region members
		[JsonProperty("id")]
		public string id { get; set; }

		[JsonProperty("level")]
		public int level { get; set; }

		[JsonProperty("requiredPlayerLevel")]
		public int requiredPlayerLevel { get; set; }

		[JsonProperty("requiredReputation")]
		public double requiredReputation { get; set; }

		[JsonProperty("requiredCommerce")]
		public int requiredCommerce { get; set; }

		[JsonProperty("payRate")]
		public double payRate { get; set; }

		[JsonProperty("insuranceRate")]
		public double? insuranceRate { get; set; }

		[JsonProperty("repairCostMultiplier")]
		public double? repairCostMultiplier { get; set; }

		[JsonProperty("barters")]
		public List<Barter> barters { get; set; }

		[JsonProperty("cashOffers")]
		public List<TraderCashOffer> cashOffers { get; set; }
		#endregion
	}
	#endregion

	#region TraderCashOffer
	public class TraderCashOffer
	{
		#region members
		[JsonProperty("item")]
		public Item item { get; set; }

		[JsonProperty("minTraderLevel")]
		public int? minTraderLevel { get; set; }

		[JsonProperty("price")]
		public int? price { get; set; }

		[JsonProperty("currency")]
		public string currency { get; set; }

		[JsonProperty("currencyItem")]
		public Item currencyItem { get; set; }

		[JsonProperty("priceRUB")]
		public int? priceRUB { get; set; }

		[JsonProperty("taskUnlock")]
		public Task taskUnlock { get; set; }
		#endregion
	}
	#endregion
	public enum TraderName
	{
		prapor,
		therapist,
		fence,
		skier,
		peacekeeper,
		mechanic,
		ragman,
		jaeger
	}


	#region TraderOffer
	public class TraderOffer : Vendor
	{
		#region members
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("normalizedName")]
		public string normalizedName { get; set; }

		[JsonProperty("trader")]
		public Trader trader { get; set; }

		[JsonProperty("minTraderLevel")]
		public int? minTraderLevel { get; set; }

		[JsonProperty("taskUnlock")]
		public Task taskUnlock { get; set; }
		#endregion
	}
	#endregion

	#region TraderStanding
	public class TraderStanding
	{
		#region members
		[JsonProperty("trader")]
		public Trader trader { get; set; }

		[JsonProperty("standing")]
		public double standing { get; set; }
		#endregion
	}
	#endregion

	public interface Vendor
	{
		[JsonProperty("name")]
		string name { get; set; }

		[JsonProperty("normalizedName")]
		string normalizedName { get; set; }
	}

	#region Query
	public class Query
	{
		#region members
		[JsonProperty("ammo")]
		public List<Ammo> ammo { get; set; }

		[JsonProperty("barters")]
		public List<Barter> barters { get; set; }

		[JsonProperty("crafts")]
		public List<Craft> crafts { get; set; }

		[JsonProperty("hideoutStations")]
		public List<HideoutStation> hideoutStations { get; set; }

		[JsonProperty("historicalItemPrices")]
		public List<historicalPricePoint> historicalItemPrices { get; set; }

		[JsonProperty("item")]
		public Item item { get; set; }

		[JsonProperty("items")]
		public List<Item> items { get; set; }

		[JsonProperty("itemCategories")]
		public List<ItemCategory> itemCategories { get; set; }

		[JsonProperty("handbookCategories")]
		public List<ItemCategory> handbookCategories { get; set; }

		[JsonProperty("maps")]
		public List<Map> maps { get; set; }

		[JsonProperty("questItems")]
		public List<QuestItem> questItems { get; set; }

		[JsonProperty("status")]
		public ServerStatus status { get; set; }

		[JsonProperty("task")]
		public Task task { get; set; }

		[JsonProperty("tasks")]
		public List<Task> tasks { get; set; }

		[JsonProperty("traders")]
		public List<Trader> traders { get; set; }

		[JsonProperty("fleaMarket")]
		public FleaMarket fleaMarket { get; set; }

		[JsonProperty("armorMaterials")]
		public List<ArmorMaterial> armorMaterials { get; set; }

		[JsonProperty("playerLevels")]
		public List<PlayerLevel> playerLevels { get; set; }

		[Obsolete("Use hideoutStations instead.")]
		[JsonProperty("hideoutModules")]
		public List<HideoutModule> hideoutModules { get; set; }

		[Obsolete("Use items instead.")]
		[JsonProperty("itemsByIDs")]
		public List<Item> itemsByIDs { get; set; }

		[Obsolete("Use items instead.")]
		[JsonProperty("itemsByType")]
		public List<Item> itemsByType { get; set; }

		[Obsolete("Use items instead.")]
		[JsonProperty("itemsByName")]
		public List<Item> itemsByName { get; set; }

		[Obsolete("Use item instead.")]
		[JsonProperty("itemByNormalizedName")]
		public Item itemByNormalizedName { get; set; }

		[Obsolete("Use items instead.")]
		[JsonProperty("itemsByBsgCategoryId")]
		public List<Item> itemsByBsgCategoryId { get; set; }

		[Obsolete("Use tasks instead.")]
		[JsonProperty("quests")]
		public List<Quest> quests { get; set; }

		[Obsolete("Use traders instead.")]
		[JsonProperty("traderResetTimes")]
		public List<TraderResetTime> traderResetTimes { get; set; }
		#endregion
	}
	#endregion

	#region ItemTranslation
	public class ItemTranslation
	{
		#region members
		[Obsolete("Use the lang argument on queries instead.")]
		[JsonProperty("name")]
		public string name { get; set; }

		[Obsolete("Use the lang argument on queries instead.")]
		[JsonProperty("shortName")]
		public string shortName { get; set; }

		[Obsolete("Use the lang argument on queries instead.")]
		[JsonProperty("description")]
		public string description { get; set; }
		#endregion
	}
	#endregion

	#region HideoutModule
	public class HideoutModule
	{
		#region members
		[Obsolete("Use HideoutStation type instead.")]
		[JsonProperty("id")]
		public int? id { get; set; }

		[Obsolete("Use HideoutStation type instead.")]
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("level")]
		public int? level { get; set; }

		[JsonProperty("itemRequirements")]
		public List<ContainedItem> itemRequirements { get; set; }

		[JsonProperty("moduleRequirements")]
		public List<HideoutModule> moduleRequirements { get; set; }
		#endregion
	}
	#endregion

	#region Quest
	public class Quest
	{
		#region members
		[Obsolete("Use Task type instead.")]
		[JsonProperty("id")]
		public string id { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("requirements")]
		public QuestRequirement requirements { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("giver")]
		public Trader giver { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("turnin")]
		public Trader turnin { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("title")]
		public string title { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("wikiLink")]
		public string wikiLink { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("exp")]
		public int exp { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("unlocks")]
		public List<string> unlocks { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("reputation")]
		public List<QuestRewardReputation> reputation { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("objectives")]
		public List<QuestObjective> objectives { get; set; }
		#endregion
	}
	#endregion

	#region QuestObjective
	public class QuestObjective
	{
		#region members
		[Obsolete("Use Task type instead.")]
		[JsonProperty("id")]
		public string id { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("type")]
		public string type { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("target")]
		public List<string> target { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("targetItem")]
		public Item targetItem { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("number")]
		public int? number { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("location")]
		public string location { get; set; }
		#endregion
	}
	#endregion

	#region QuestRequirement
	public class QuestRequirement
	{
		#region members
		[Obsolete("Use Task type instead.")]
		[JsonProperty("level")]
		public int? level { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("quests")]
		public List<List<int?>> quests { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("prerequisiteQuests")]
		public List<List<Quest>> prerequisiteQuests { get; set; }
		#endregion
	}
	#endregion

	#region QuestRewardReputation
	public class QuestRewardReputation
	{
		#region members
		[Obsolete("Use Task type instead.")]
		[JsonProperty("trader")]
		public Trader trader { get; set; }

		[Obsolete("Use Task type instead.")]
		[JsonProperty("amount")]
		public double amount { get; set; }
		#endregion
	}
	#endregion

	#region TraderPrice
	public class TraderPrice
	{
		#region members
		[Obsolete("Use item.buyFor instead.")]
		[JsonProperty("price")]
		public int price { get; set; }

		[Obsolete("Use item.buyFor instead.")]
		[JsonProperty("currency")]
		public string currency { get; set; }

		[Obsolete("Use item.buyFor instead.")]
		[JsonProperty("priceRUB")]
		public int priceRUB { get; set; }

		[Obsolete("Use item.buyFor instead.")]
		[JsonProperty("trader")]
		public Trader trader { get; set; }
		#endregion
	}
	#endregion

	#region TraderResetTime
	public class TraderResetTime
	{
		#region members
		[Obsolete("Use Trader.name type instead.")]
		[JsonProperty("name")]
		public string name { get; set; }

		[Obsolete("Use Trader.resetTime type instead.")]
		[JsonProperty("resetTimestamp")]
		public string resetTimestamp { get; set; }
		#endregion
	}
	#endregion
	public enum ItemType
	{
		ammo,
		ammoBox,
		any,
		armor,
		backpack,
		barter,
		container,
		glasses,
		grenade,
		gun,
		headphones,
		helmet,
		injectors,
		keys,
		markedOnly,
		meds,
		mods,
		noFlea,
		pistolGrip,
		preset,
		provisions,
		rig,
		suppressor,
		wearable
	}

	public enum ItemCategoryName
	{
		Ammo,
		AmmoContainer,
		ArmBand,
		Armor,
		ArmoredEquipment,
		AssaultCarbine,
		AssaultRifle,
		AssaultScope,
		AuxiliaryMod,
		Backpack,
		Barrel,
		BarterItem,
		Battery,
		Bipod,
		BuildingMaterial,
		ChargingHandle,
		ChestRig,
		CombMuzzleDevice,
		CombTactDevice,
		CommonContainer,
		CompactReflexSight,
		Compass,
		CompoundItem,
		CylinderMagazine,
		Drink,
		Drug,
		Electronics,
		Equipment,
		EssentialMod,
		FaceCover,
		Flashhider,
		Flashlight,
		Food,
		FoodAndDrink,
		Foregrip,
		Fuel,
		FunctionalMod,
		GasBlock,
		GearMod,
		GrenadeLauncher,
		Handguard,
		Handgun,
		Headphones,
		Headwear,
		HouseholdGoods,
		Info,
		Ironsight,
		Item,
		Jewelry,
		Key,
		KeyMechanical,
		Keycard,
		Knife,
		LockingContainer,
		Lubricant,
		Machinegun,
		Magazine,
		Map,
		MarksmanRifle,
		MedicalItem,
		MedicalSupplies,
		Medikit,
		Meds,
		Money,
		Mount,
		MuzzleDevice,
		NightVision,
		Other,
		PistolGrip,
		PortContainer,
		PortableRangeFinder,
		RadioTransmitter,
		RandomLootContainer,
		Receiver,
		ReflexSight,
		RepairKits,
		Revolver,
		SMG,
		Scope,
		SearchableItem,
		Shotgun,
		Sights,
		Silencer,
		SniperRifle,
		SpecialItem,
		SpecialScope,
		SpringDrivenCylinder,
		StackableItem,
		Stimulant,
		Stock,
		ThermalVision,
		ThrowableWeapon,
		Tool,
		UBGL,
		VisObservDevice,
		Weapon,
		WeaponMod
	}

	public enum HandbookCategoryName
	{
		Ammo,
		AmmoPacks,
		AssaultCarbines,
		AssaultRifles,
		AssaultScopes,
		AuxiliaryParts,
		Backpacks,
		Barrels,
		BarterItems,
		Bipods,
		BodyArmor,
		BoltActionRifles,
		BuildingMaterials,
		ChargingHandles,
		Collimators,
		CompactCollimators,
		Drinks,
		ElectronicKeys,
		Electronics,
		EnergyElements,
		Eyewear,
		Facecovers,
		FlammableMaterials,
		FlashhidersBrakes,
		Flashlights,
		Food,
		Foregrips,
		FunctionalMods,
		GasBlocks,
		Gear,
		GearComponents,
		GearMods,
		GrenadeLaunchers,
		Handguards,
		Headgear,
		Headsets,
		HouseholdMaterials,
		InfoItems,
		Injectors,
		InjuryTreatment,
		IronSights,
		Keys,
		LaserTargetPointers,
		Launchers,
		LightLaserDevices,
		MachineGuns,
		Magazines,
		Maps,
		MarksmanRifles,
		MechanicalKeys,
		MedicalSupplies,
		Medication,
		Medkits,
		MeleeWeapons,
		Money,
		Mounts,
		MuzzleAdapters,
		MuzzleDevices,
		Optics,
		Others,
		Pills,
		PistolGrips,
		Pistols,
		Provisions,
		ReceiversSlides,
		Rounds,
		SMGs,
		SecureContainers,
		Shotguns,
		Sights,
		SpecialEquipment,
		SpecialPurposeSights,
		SpecialWeapons,
		StocksChassis,
		StorageContainers,
		Suppressors,
		TacticalComboDevices,
		TacticalRigs,
		Throwables,
		Tools,
		Valuables,
		VitalParts,
		WeaponPartsMods,
		Weapons
	}

	public enum LanguageCode
	{
		cs,
		de,
		en,
		es,
		fr,
		hu,
		it,
		ja,
		ko,
		pl,
		pt,
		ru,
		sk,
		tr,
		zh
	}

}
