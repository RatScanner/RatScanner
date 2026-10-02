using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RatScanner.TarkovDev.Json;

/// <summary>
/// Reads a field the API sends as either a bool or a string. Newtonsoft would
/// otherwise coerce a <c>false</c> into the literal text "False", which is
/// indistinguishable from a real id at the call site.
/// </summary>
internal class BoolOrStringConverter : JsonConverter<string?> {
	public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer) {
		switch (reader.TokenType) {
			case JsonToken.Null:
				return null;
			// A bool here means "not controlled", not an id.
			case JsonToken.Boolean:
				return null;
			case JsonToken.String:
				var s = (string?)reader.Value;
				return string.IsNullOrWhiteSpace(s) ? null : s;
			default:
				return reader.Value?.ToString();
		}
	}

	public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer) {
		if (value == null) {
			writer.WriteNull();
		} else {
			writer.WriteValue(value);
		}
	}
}

public class Map {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Extraction points on this map. Empty when the API has none for the
	/// current game mode, e.g. maps without a scav raid.
	/// </summary>
	[JsonProperty("extracts")]
	public List<Extract> Extracts { get; set; } = [];

	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; } = string.Empty;

	[JsonProperty("nameId")]
	public string NameId { get; set; } = string.Empty;

	[JsonProperty("description")]
	public string Description { get; set; } = string.Empty;

	[JsonProperty("scenePath")]
	public string ScenePath { get; set; } = string.Empty;

	[JsonProperty("wiki")]
	public string? Wiki { get; set; }

	[JsonProperty("enemies")]
	public List<string> Enemies { get; set; } = [];

	[JsonProperty("raidDuration")]
	public int RaidDuration { get; set; }

	[JsonProperty("players")]
	public string Players { get; set; } = string.Empty;

	/// <summary>
	/// Boss and boss-escort groups that may spawn on this map.
	/// </summary>
	[JsonProperty("bosses")]
	public List<Boss> Bosses { get; set; } = [];

	/// <summary>
	/// Compass rotation applied to this map's coordinates, in degrees.
	/// <see cref="MapProjection"/> already accounts for it via the projection
	/// data, so this is informational only.
	/// </summary>
	[JsonProperty("coordinateToCardinalRotation")]
	public double CoordinateToCardinalRotation { get; set; }

	/// <summary>
	/// Points where players, bots or bosses can spawn. A spawn's
	/// <see cref="Spawn.Categories"/> and <see cref="Spawn.Sides"/> say what may
	/// appear there.
	/// </summary>
	[JsonProperty("spawns")]
	public List<Spawn> Spawns { get; set; } = [];

	/// <summary>
	/// Passages between maps, e.g. the underground tunnels.
	/// </summary>
	[JsonProperty("transits")]
	public List<Transit> Transits { get; set; } = [];

	/// <summary>
	/// Keyed doors and trunks, and what opens them.
	/// </summary>
	[JsonProperty("locks")]
	public List<Lock> Locks { get; set; } = [];

	/// <summary>
	/// Environmental hazards, e.g. minefields and sniper zones.
	/// </summary>
	[JsonProperty("hazards")]
	public List<Hazard> Hazards { get; set; } = [];

	/// <summary>
	/// Placed loot containers. The id resolves against the items endpoint to get
	/// the container's contents.
	/// </summary>
	[JsonProperty("lootContainers")]
	public List<MapLootContainer> LootContainers { get; set; } = [];

	/// <summary>
	/// Items lying on the floor, with the items in each pile.
	/// </summary>
	[JsonProperty("lootLoose")]
	public List<MapLootLoose> LootLoose { get; set; } = [];

	/// <summary>
	/// Levers and buttons that operate other objects.
	/// </summary>
	[JsonProperty("switches")]
	public List<Switch> Switches { get; set; } = [];

	/// <summary>
	/// Mounted weapons, referenced by item id.
	/// </summary>
	[JsonProperty("stationaryWeapons")]
	public List<MapStationaryWeapon> StationaryWeapons { get; set; } = [];

	/// <summary>
	/// BTR taxi pickup and drop-off points.
	/// </summary>
	[JsonProperty("btrStops")]
	public List<BtrStop> BtrStops { get; set; } = [];

	[JsonProperty("minPlayerLevel")]
	public int MinPlayerLevel { get; set; }

	[JsonProperty("maxPlayerLevel")]
	public int MaxPlayerLevel { get; set; }

	/// <summary>
	/// Item ids of keys that open the map at the start of a raid.
	/// </summary>
	[JsonProperty("accessKeys")]
	public List<string> AccessKeys { get; set; } = [];

	/// <summary>
	/// Minimum player level required to use <see cref="AccessKeys"/>.
	/// </summary>
	[JsonProperty("accessKeysMinPlayerLevel")]
	public int AccessKeysMinPlayerLevel { get; set; }
}

/// <summary>
/// An extraction point on a map.
/// </summary>
public class Extract {
	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("faction")]
	public string Faction { get; set; } = string.Empty;

	/// <summary>
	/// The extract's position in the game's 3D world space, not as a percentage
	/// of the map. Convert it with MapPoiExtensions.GetExtractPois, which applies
	/// the per-map projection from maps.json.
	/// </summary>
	[JsonProperty("position")]
	public WorldPosition? Position { get; set; }

	/// <summary>
	/// Height of the top of the extract, in world units. Negative values mean the
	/// extract is on an underground level.
	/// </summary>
	[JsonProperty("top")]
	public double Top { get; set; }

	/// <summary>
	/// Height of the bottom of the extract, in world units.
	/// </summary>
	[JsonProperty("bottom")]
	public double Bottom { get; set; }
}

/// <summary>
/// A point in the game's 3D world space. Shared by everything placed on a map,
/// since they all use the same coordinate space.
/// </summary>
public class WorldPosition {
	[JsonProperty("x")]
	public double X { get; set; }

	/// <summary>
	/// Vertical height. Ignored for positioning, since the maps are drawn from
	/// above and a marker would only jump when it changed floors.
	/// </summary>
	[JsonProperty("y")]
	public double Y { get; set; }

	[JsonProperty("z")]
	public double Z { get; set; }
}

/// <summary>
/// The extent of an object on one axis, in world units.
/// </summary>
public class WorldSize {
	[JsonProperty("x")]
	public double X { get; set; }

	[JsonProperty("y")]
	public double Y { get; set; }

	[JsonProperty("z")]
	public double Z { get; set; }
}

/// <summary>
/// A boss, or a group of escorts, that can spawn on a map.
/// </summary>
public class Boss {
	/// <summary>
	/// Mob id, e.g. "bossTagilla". Resolve it against the mobs collection on the
	/// same endpoint, or against the items endpoint.
	/// </summary>
	[JsonProperty("mob")]
	public string Mob { get; set; } = string.Empty;

	/// <summary>
	/// Chance of this group spawning at all, from 0 to 1.
	/// </summary>
	[JsonProperty("spawnChance")]
	public double SpawnChance { get; set; }

	/// <summary>
	/// Named areas the group can spawn in, each with its own chance and
	/// candidate positions.
	/// </summary>
	[JsonProperty("spawnLocations")]
	public List<BossSpawnLocation> SpawnLocations { get; set; } = [];

	/// <summary>
	/// Escorts that accompany the boss.
	/// </summary>
	[JsonProperty("escorts")]
	public List<BossEscort> Escorts { get; set; } = [];

	/// <summary>
	/// Support mobs that accompany the boss.
	/// </summary>
	[JsonProperty("supports")]
	public List<BossEscort> Supports { get; set; } = [];

	/// <summary>
	/// Seconds into the raid after which this group can spawn. -1 means there is
	/// no time restriction.
	/// </summary>
	[JsonProperty("spawnTime")]
	public int SpawnTime { get; set; }

	/// <summary>
	/// Whether <see cref="SpawnTime"/> is randomised within its window.
	/// </summary>
	[JsonProperty("spawnTimeRandom")]
	public bool SpawnTimeRandom { get; set; }

	/// <summary>
	/// What causes the group to spawn, e.g. "Switch". Null when it is not
	/// conditional.
	/// </summary>
	[JsonProperty("spawnTrigger")]
	public string? SpawnTrigger { get; set; }

	/// <summary>
	/// Id of the switch that triggers this group. Only set when
	/// <see cref="SpawnTrigger"/> is "Switch".
	/// </summary>
	[JsonProperty("switch")]
	public string? SwitchId { get; set; }

	[JsonProperty("switch_id")]
	public string? SwitchName { get; set; }
}

/// <summary>
/// A named area a boss group can spawn in.
/// </summary>
public class BossSpawnLocation {
	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Chance of spawning in this area, from 0 to 1.
	/// </summary>
	[JsonProperty("chance")]
	public double Chance { get; set; }

	/// <summary>
	/// Key tying this area back to the map's spawn zones.
	/// </summary>
	[JsonProperty("spawnKey")]
	public string SpawnKey { get; set; } = string.Empty;

	/// <summary>
	/// Candidate positions within the area. Any one of them may be used.
	/// </summary>
	[JsonProperty("positions")]
	public List<WorldPosition> Positions { get; set; } = [];
}

/// <summary>
/// An escort or support mob accompanying a boss.
/// </summary>
public class BossEscort {
	[JsonProperty("mob")]
	public string Mob { get; set; } = string.Empty;

	/// <summary>
	/// Possible escort counts and the chance of each.
	/// </summary>
	[JsonProperty("amount")]
	public List<BossEscortAmount> Amount { get; set; } = [];
}

/// <summary>
/// One possible escort count and its chance.
/// </summary>
public class BossEscortAmount {
	/// <summary>Chance of this count, from 0 to 1.</summary>
	[JsonProperty("chance")]
	public double Chance { get; set; }

	/// <summary>How many escorts spawn.</summary>
	[JsonProperty("count")]
	public int Count { get; set; }
}

/// <summary>
/// A point where something can spawn.
/// </summary>
public class Spawn {
	/// <summary>
	/// The spawn point in world space.
	/// </summary>
	[JsonProperty("position")]
	public WorldPosition Position { get; set; } = new();

	/// <summary>
	/// Which sides this spawn applies to. Values seen: "all", "none", "pmc",
	/// "scav".
	/// </summary>
	[JsonProperty("sides")]
	public List<string> Sides { get; set; } = [];

	/// <summary>
	/// What can spawn here. Values seen include "all", "player", "boss", "bot",
	/// "botpmc", "sniper" and "season01".
	/// </summary>
	[JsonProperty("categories")]
	public List<string> Categories { get; set; } = [];

	/// <summary>
	/// Id of the spawn zone this point belongs to.
	/// </summary>
	[JsonProperty("zoneName")]
	public string ZoneName { get; set; } = string.Empty;
}

/// <summary>
/// A passage between maps.
/// </summary>
public class Transit {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Translation key for the transit's name, e.g. "FAC_TRANSIT_12_DESC".
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Id of the map this transit leads to.
	/// </summary>
	[JsonProperty("map")]
	public string Map { get; set; } = string.Empty;

	[JsonProperty("position")]
	public WorldPosition Position { get; set; } = new();

	/// <summary>
	/// Translation key for the conditions under which the transit is open. Null
	/// when it is always open.
	/// </summary>
	[JsonProperty("conditions")]
	public string? Conditions { get; set; }

	/// <summary>
	/// Polygon outlining the entrance. Usually four points.
	/// </summary>
	[JsonProperty("outline")]
	public List<WorldPosition> Outline { get; set; } = [];

	/// <summary>Height of the top of the entrance, in world units.</summary>
	[JsonProperty("top")]
	public double Top { get; set; }

	/// <summary>Height of the bottom of the entrance, in world units.</summary>
	[JsonProperty("bottom")]
	public double Bottom { get; set; }

	/// <summary>
	/// Extent of the entrance, when the API provides it.
	/// </summary>
	[JsonProperty("size")]
	public WorldSize? Size { get; set; }
}

/// <summary>
/// A door or trunk, and what opens it.
/// </summary>
public class Lock {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>What kind of lock this is. Values seen: "door", "trunk".</summary>
	[JsonProperty("lockType")]
	public string LockType { get; set; } = string.Empty;

	/// <summary>
	/// Item id of the key that opens it. Empty when the lock needs no key.
	/// </summary>
	[JsonProperty("key")]
	public string Key { get; set; } = string.Empty;

	/// <summary>Whether the lock must be powered before it will open.</summary>
	[JsonProperty("needsPower")]
	public bool NeedsPower { get; set; }

	[JsonProperty("position")]
	public WorldPosition Position { get; set; } = new();

	/// <summary>Height of the top of the lock, in world units.</summary>
	[JsonProperty("top")]
	public double Top { get; set; }

	/// <summary>Height of the bottom of the lock, in world units.</summary>
	[JsonProperty("bottom")]
	public double Bottom { get; set; }

	/// <summary>Polygon outlining the lock, when the API provides it.</summary>
	[JsonProperty("outline")]
	public List<WorldPosition> Outline { get; set; } = [];

	/// <summary>Extent of the lock, when the API provides it.</summary>
	[JsonProperty("size")]
	public WorldSize? Size { get; set; }
}

/// <summary>
/// An environmental hazard on a map.
/// </summary>
public class Hazard {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// What kind of hazard this is. Values seen: "hazard", "minefield", "sniper".
	/// </summary>
	[JsonProperty("hazardType")]
	public string HazardType { get; set; } = string.Empty;

	/// <summary>
	/// Damage type key, e.g. "DamageType_Landmine".
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("position")]
	public WorldPosition Position { get; set; } = new();

	/// <summary>
	/// Polygon outlining the hazard's area.
	/// </summary>
	[JsonProperty("outline")]
	public List<WorldPosition> Outline { get; set; } = [];

	/// <summary>Height of the top of the hazard, in world units.</summary>
	[JsonProperty("top")]
	public double Top { get; set; }

	/// <summary>Height of the bottom of the hazard, in world units.</summary>
	[JsonProperty("bottom")]
	public double Bottom { get; set; }

	/// <summary>Extent of the hazard, when the API provides it.</summary>
	[JsonProperty("size")]
	public WorldSize? Size { get; set; }
}

/// <summary>
/// A placed loot container.
/// </summary>
public class MapLootContainer {
	/// <summary>
	/// Item id of the container. Resolve it against the items endpoint to get its
	/// contents.
	/// </summary>
	[JsonProperty("lootContainer")]
	public string LootContainer { get; set; } = string.Empty;

	[JsonProperty("position")]
	public WorldPosition Position { get; set; } = new();
}

/// <summary>
/// A pile of items lying on the floor.
/// </summary>
public class MapLootLoose {
	[JsonProperty("position")]
	public WorldPosition Position { get; set; } = new();

	/// <summary>
	/// Item ids in the pile. Resolve them against the items endpoint.
	/// </summary>
	[JsonProperty("items")]
	public List<string> Items { get; set; } = [];
}

/// <summary>
/// A lever, button or other switch that operates other objects.
/// </summary>
public class Switch {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Internal name of the switch, e.g.
	/// "switch_custom_DesignStuff_00034_reserve_electric_switcher_lever".
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Name of the door this switch controls, when it controls one.
	/// </summary>
	[JsonProperty("door")]
	public string? Door { get; set; }

	/// <summary>
	/// What the switch does, e.g. "Open".
	/// </summary>
	[JsonProperty("switchType")]
	public string SwitchType { get; set; } = string.Empty;

	/// <summary>
	/// Id of whatever operates this switch, when another switch controls it. Null
	/// when it is not controlled. The API sends a bool here instead of an id when
	/// the switch is not controlled, which the converter normalises to null.
	/// </summary>
	[JsonProperty("activatedBy")]
	[JsonConverter(typeof(BoolOrStringConverter))]
	public string? ActivatedBy { get; set; }

	/// <summary>
	/// Switches and doors this one operates.
	/// </summary>
	[JsonProperty("activates")]
	public List<SwitchActivation> Activates { get; set; } = [];

	[JsonProperty("position")]
	public WorldPosition Position { get; set; } = new();

	[JsonProperty("outline")]
	public List<WorldPosition> Outline { get; set; } = [];

	[JsonProperty("top")]
	public double Top { get; set; }

	[JsonProperty("bottom")]
	public double Bottom { get; set; }

	[JsonProperty("size")]
	public WorldSize? Size { get; set; }
}

/// <summary>
/// One thing a switch operates.
/// </summary>
public class SwitchActivation {
	/// <summary>What it does, e.g. "Unlock" or "Lock".</summary>
	[JsonProperty("operation")]
	public string Operation { get; set; } = string.Empty;

	/// <summary>Id of the switch or door being operated.</summary>
	[JsonProperty("switch")]
	public string Switch { get; set; } = string.Empty;
}

/// <summary>
/// A mounted weapon.
/// </summary>
public class MapStationaryWeapon {
	/// <summary>
	/// Item id of the weapon. Resolve it against the items endpoint.
	/// </summary>
	[JsonProperty("stationaryWeapon")]
	public string StationaryWeapon { get; set; } = string.Empty;

	[JsonProperty("position")]
	public WorldPosition Position { get; set; } = new();
}

/// <summary>
/// A BTR taxi pickup or drop-off point. The coordinates are inline rather than
/// a nested position object, unlike everything else on a map.
/// </summary>
public class BtrStop {
	/// <summary>
	/// Translation key for the stop's name, e.g.
	/// "Trading/Dialog/PlayerTaxi/Woods/p5/Name".
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("x")]
	public double X { get; set; }

	[JsonProperty("y")]
	public double Y { get; set; }

	[JsonProperty("z")]
	public double Z { get; set; }

	/// <summary>The stop as a <see cref="WorldPosition"/>.</summary>
	public WorldPosition ToWorldPosition() => new() { X = X, Y = Y, Z = Z };
}