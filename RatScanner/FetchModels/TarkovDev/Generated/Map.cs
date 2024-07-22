using System.Collections.Generic;
using MudBlazor;
using Newtonsoft.Json;
using RatScanner.FetchModels.TarkovDev.Generated;

namespace RatScanner.FetchModels.TarkovDev;

public class Map
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("tarkovDataId")]
	public string TarkovDataId { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("wiki")]
	public string Wiki { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("enemies")]
	public List<string> Enemies { get; set; }

	[JsonProperty("raidDuration")]
	public int? RaidDuration { get; set; }

	[JsonProperty("players")]
	public string Players { get; set; }

	[JsonProperty("bosses")]
	public List<BossSpawn> Bosses { get; set; }

	[JsonProperty("nameId")]
	public string NameId { get; set; }

	[JsonProperty("accessKeys")]
	public List<Item> AccessKeys { get; set; }

	[JsonProperty("accessKeysMinPlayerLevel")]
	public int? AccessKeysMinPlayerLevel { get; set; }

	[JsonProperty("spawns")]
	public List<MapSpawn> Spawns { get; set; }

	[JsonProperty("extracts")]
	public List<MapExtract> Extracts { get; set; }

	[JsonProperty("locks")]
	public List<Lock> Locks { get; set; }

	[JsonProperty("switches")]
	public List<MapSwitch> Switches { get; set; }

	[JsonProperty("hazards")]
	public List<MapHazard> Hazards { get; set; }

	[JsonProperty("lootContainers")]
	public List<LootContainerPosition> LootContainers { get; set; }

	[JsonProperty("stationaryWeapons")]
	public List<StationaryWeaponPosition> StationaryWeapons { get; set; }
}
