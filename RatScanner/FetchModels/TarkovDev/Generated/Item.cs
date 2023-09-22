using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Item
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("shortName")]
	public string ShortName { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("basePrice")]
	public int BasePrice { get; set; }

	[JsonProperty("updated")]
	public string Updated { get; set; }

	[JsonProperty("width")]
	public int Width { get; set; }

	[JsonProperty("height")]
	public int Height { get; set; }

	[JsonProperty("backgroundColor")]
	public string BackgroundColor { get; set; }

	[JsonProperty("iconLink")]
	public string IconLink { get; set; }

	[JsonProperty("gridImageLink")]
	public string GridImageLink { get; set; }

	[JsonProperty("baseImageLink")]
	public string BaseImageLink { get; set; }

	[JsonProperty("inspectImageLink")]
	public string InspectImageLink { get; set; }

	[JsonProperty("image512pxLink")]
	public string Image512PxLink { get; set; }

	[JsonProperty("image8xLink")]
	public string Image8XLink { get; set; }

	[JsonProperty("wikiLink")]
	public string WikiLink { get; set; }

	[JsonProperty("types")]
	public List<ItemType?> Types { get; set; }

	[JsonProperty("avg24hPrice")]
	public int? Avg24HPrice { get; set; }

	[JsonProperty("properties")]
	public ItemProperties Properties { get; set; }

	[JsonProperty("conflictingItems")]
	public List<Item> ConflictingItems { get; set; }

	[JsonProperty("conflictingSlotIds")]
	public List<string> ConflictingSlotIds { get; set; }

	[JsonProperty("accuracyModifier")]
	public double? AccuracyModifier { get; set; }

	[JsonProperty("recoilModifier")]
	public double? RecoilModifier { get; set; }

	[JsonProperty("ergonomicsModifier")]
	public double? ErgonomicsModifier { get; set; }

	[JsonProperty("hasGrid")]
	public bool? HasGrid { get; set; }

	[JsonProperty("blocksHeadphones")]
	public bool? BlocksHeadphones { get; set; }

	[JsonProperty("link")]
	public string Link { get; set; }

	[JsonProperty("lastLowPrice")]
	public int? LastLowPrice { get; set; }

	[JsonProperty("changeLast48h")]
	public double? ChangeLast48H { get; set; }

	[JsonProperty("changeLast48hPercent")]
	public double? ChangeLast48HPercent { get; set; }

	[JsonProperty("low24hPrice")]
	public int? Low24HPrice { get; set; }

	[JsonProperty("high24hPrice")]
	public int? High24HPrice { get; set; }

	[JsonProperty("lastOfferCount")]
	public int? LastOfferCount { get; set; }

	[JsonProperty("sellFor")]
	public List<ItemPrice> SellFor { get; set; }

	[JsonProperty("buyFor")]
	public List<ItemPrice> BuyFor { get; set; }

	[JsonProperty("containsItems")]
	public List<ContainedItem> ContainsItems { get; set; }

	[JsonProperty("category")]
	public ItemCategory Category { get; set; }

	[JsonProperty("categories")]
	public List<ItemCategory> Categories { get; set; }

	[JsonProperty("bsgCategoryId")]
	public string BsgCategoryId { get; set; }

	[JsonProperty("handbookCategories")]
	public List<ItemCategory> HandbookCategories { get; set; }

	[JsonProperty("weight")]
	public double? Weight { get; set; }

	[JsonProperty("velocity")]
	public double? Velocity { get; set; }

	[JsonProperty("loudness")]
	public int? Loudness { get; set; }

	[JsonProperty("usedInTasks")]
	public List<Task> UsedInTasks { get; set; }

	[JsonProperty("receivedFromTasks")]
	public List<Task> ReceivedFromTasks { get; set; }

	[JsonProperty("bartersFor")]
	public List<Barter> BartersFor { get; set; }

	[JsonProperty("bartersUsing")]
	public List<Barter> BartersUsing { get; set; }

	[JsonProperty("craftsFor")]
	public List<Craft> CraftsFor { get; set; }

	[JsonProperty("craftsUsing")]
	public List<Craft> CraftsUsing { get; set; }

	[JsonProperty("historicalPrices")]
	public List<HistoricalPricePoint> HistoricalPrices { get; set; }

	[JsonProperty("fleaMarketFee")]
	public int? FleaMarketFee { get; set; }

	[Obsolete("No longer meaningful with inclusion of Item category.")]
	[JsonProperty("categoryTop")]
	public ItemCategory CategoryTop { get; set; }

	[Obsolete("Use the lang argument on queries instead.")]
	[JsonProperty("translation")]
	public ItemTranslation Translation { get; set; }

	[Obsolete("Use sellFor instead.")]
	[JsonProperty("traderPrices")]
	public List<TraderPrice> TraderPrices { get; set; }

	[Obsolete("Use category instead.")]
	[JsonProperty("bsgCategory")]
	public ItemCategory BsgCategory { get; set; }

	[Obsolete("Use inspectImageLink instead.")]
	[JsonProperty("imageLink")]
	public string ImageLink { get; set; }

	[Obsolete("Fallback handled automatically by inspectImageLink.")]
	[JsonProperty("imageLinkFallback")]
	public string ImageLinkFallback { get; set; }

	[Obsolete("Fallback handled automatically by iconLink.")]
	[JsonProperty("iconLinkFallback")]
	public string IconLinkFallback { get; set; }

	[Obsolete("Fallback handled automatically by gridImageLink.")]
	[JsonProperty("gridImageLinkFallback")]
	public string GridImageLinkFallback { get; set; }
}
