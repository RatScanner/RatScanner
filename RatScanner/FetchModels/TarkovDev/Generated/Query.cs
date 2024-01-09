using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Query
{
	[JsonProperty("ammo")]
	public List<Ammo> Ammo { get; set; }

	[JsonProperty("barters")]
	public List<Barter> Barters { get; set; }

	[JsonProperty("crafts")]
	public List<Craft> Crafts { get; set; }

	[JsonProperty("hideoutStations")]
	public List<HideoutStation> HideoutStations { get; set; }

	[JsonProperty("historicalItemPrices")]
	public List<HistoricalPricePoint> HistoricalItemPrices { get; set; }

	[JsonProperty("item")]
	public Item Item { get; set; }

	[JsonProperty("items")]
	public List<Item> Items { get; set; }

	[JsonProperty("itemCategories")]
	public List<ItemCategory> ItemCategories { get; set; }

	[JsonProperty("handbookCategories")]
	public List<ItemCategory> HandbookCategories { get; set; }

	[JsonProperty("maps")]
	public List<Map> Maps { get; set; }

	[JsonProperty("questItems")]
	public List<QuestItem> QuestItems { get; set; }

	[JsonProperty("status")]
	public ServerStatus Status { get; set; }

	[JsonProperty("task")]
	public Task Task { get; set; }

	[JsonProperty("tasks")]
	public List<Task> Tasks { get; set; }

	[JsonProperty("traders")]
	public List<Trader> Traders { get; set; }

	[JsonProperty("fleaMarket")]
	public FleaMarket FleaMarket { get; set; }

	[JsonProperty("armorMaterials")]
	public List<ArmorMaterial> ArmorMaterials { get; set; }

	[JsonProperty("playerLevels")]
	public List<PlayerLevel> PlayerLevels { get; set; }
}
