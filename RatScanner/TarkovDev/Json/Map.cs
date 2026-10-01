using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.TarkovDev.Json;

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

	[JsonProperty("bosses")]
	public List<object> Bosses { get; set; } = [];
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
	public ExtractPosition? Position { get; set; }

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
/// A point in the game's 3D world space.
/// </summary>
public class ExtractPosition {
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
