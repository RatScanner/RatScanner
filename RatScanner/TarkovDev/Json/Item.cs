using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RatScanner.TarkovDev.Json;

public partial class Item {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("shortName")]
	public string ShortName { get; set; } = string.Empty;

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; } = string.Empty;

	[JsonProperty("description")]
	public string Description { get; set; } = string.Empty;

	[JsonProperty("updated")]
	public string UpdatedStr { get; set; } = string.Empty;
	public DateTime Updated => DateTime.ParseExact(UpdatedStr, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToLocalTime();

	[JsonProperty("width")]
	public int Width { get; set; }

	[JsonProperty("height")]
	public int Height { get; set; }

	[JsonProperty("weight")]
	public float Weight { get; set; }

	[JsonProperty("lastOfferCount")]
	public int? LastOfferCount { get; set; }

	[JsonProperty("types")]
	public List<string> Types { get; set; } = new();

	[JsonProperty("wikiLink")]
	public string? WikiLink { get; set; }

	[JsonProperty("link")]
	public string? Link { get; set; }

	[JsonProperty("iconLink")]
	public string? IconLink { get; set; }

	[JsonProperty("gridImageLink")]
	public string? GridImageLink { get; set; }

	[JsonProperty("baseImageLink")]
	public string? BaseImageLink { get; set; }

	[JsonProperty("inspectImageLink")]
	public string? InspectImageLink { get; set; }

	[JsonProperty("image512pxLink")]
	public string? Image512PxLink { get; set; }

	[JsonProperty("image8xLink")]
	public string? Image8XLink { get; set; }

	[JsonProperty("containsItems")]
	public List<ContainedItem> ContainsItems { get; set; } = new();

	[JsonProperty("discardLimit")]
	public int? DiscardLimit { get; set; }

	[JsonProperty("basePrice")]
	public int? BasePrice { get; set; }

	[JsonProperty("categories")]
	public List<string> Categories { get; set; } = new();

	[JsonProperty("handbookCategories")]
	public List<string> HandbookCategories { get; set; } = new();

	[JsonProperty("lastLowPrice")]
	public int? LastLowPrice { get; set; }

	[JsonProperty("avg24hPrice")]
	public int? Avg24HPrice { get; set; }

	[JsonProperty("low24hPrice")]
	public int? Low24HPrice { get; set; }

	[JsonProperty("high24hPrice")]
	public int? High24HPrice { get; set; }

	[JsonProperty("changeLast48h")]
	public int? ChangeLast48H { get; set; }

	[JsonProperty("changeLast48hPercent")]
	public float? ChangeLast48HPercent { get; set; }

	[JsonProperty("lastScan")]
	public string? LastScan { get; set; }

	[JsonProperty("properties")]
	public ItemProperties? Properties { get; set; }

	[JsonProperty("minLevelForFlea")]
	public int? MinLevelForFlea { get; set; }

	[JsonProperty("maxDurability")]
	public int? MaxDurability { get; set; }

	[JsonProperty("ergonomicsModifier")]
	public float? ErgonomicsModifier { get; set; }

	[JsonProperty("stackMaxSize")]
	public int? StackMaxSize { get; set; }

	[JsonProperty("velocity")]
	public float? Velocity { get; set; }

	[JsonProperty("hasGrid")]
	public bool? HasGrid { get; set; }

	[JsonProperty("backgroundColor")]
	public string? BackgroundColor { get; set; }

	[JsonProperty("conflictingItems")]
	public List<string> ConflictingItems { get; set; } = new();

	[JsonProperty("conflictingSlotIds")]
	public List<string> ConflictingSlotIds { get; set; } = new();

	[JsonProperty("conflictingCategories")]
	public List<string> ConflictingCategories { get; set; } = new();

	[JsonProperty("buyFromTrader")]
	public List<TraderPrice> BuyFromTrader { get; set; } = new();

	[JsonProperty("sellToTrader")]
	public List<TraderPrice> SellToTrader { get; set; } = new();
}
