using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using static RatScanner.InteractiveMapData;

namespace RatScanner;

public static class MapDataLoader {
	private static Dictionary<string, Map>? _mapsByIdCache;

	/// <summary>
	/// Loads and caches the maps.json data
	/// </summary>
	public static Dictionary<string, Map>? GetMapsData() {
		if (_mapsByIdCache != null) return _mapsByIdCache;

		try {
			var mapsJsonPath = Path.Combine(RatConfig.Paths.Data, "maps.json");
			if (!File.Exists(mapsJsonPath)) {
				Logger.LogError($"maps.json not found at {mapsJsonPath}");
				return [];
			}

			var json = File.ReadAllText(mapsJsonPath);
			var mapsData = JsonConvert.DeserializeObject<List<InteractiveMapData>>(json) ?? [];

			// Build the map ID cache
			_mapsByIdCache = BuildMapIdCache(mapsData);

			Logger.LogInfo($"Loaded {_mapsByIdCache.Count} maps from maps.json");
			return _mapsByIdCache;
		} catch (Exception e) {
			Logger.LogError("Failed to load maps.json", e);
			return [];
		}
	}

	/// <summary>
	/// Builds a dictionary mapping map IDs to their InteractiveMapData
	/// </summary>
	private static Dictionary<string, Map> BuildMapIdCache(List<InteractiveMapData> mapsData) {
		Dictionary<string, Map> cache = [];

		// Fetched once instead of per entry: this walks every entry in the file,
		// and each call would otherwise rebuild the map list.
		var tarkovMaps = TarkovDevAPI.GetMaps().ToList();

		foreach (var mapData in mapsData) {
			if (mapData.Maps == null) continue;
			if (string.IsNullOrEmpty(mapData.NormalizedName)) continue;

			foreach (var map in mapData.Maps) {
				if (string.IsNullOrEmpty(map.Key)) continue;
				if (map.Projection != "interactive") continue;
				if (string.IsNullOrEmpty(map.SvgPath)) continue;

				// Index this entry against every tarkov.dev map it can serve.
				// Exact matches win; a prefix match is the fallback for variants
				// that share an artwork with their base map.
				foreach (var tMap in ResolveTarkovMaps(tarkovMaps, mapData.NormalizedName)) {
					if (string.IsNullOrEmpty(tMap.Id)) continue;
					cache[tMap.Id] = map;
				}
			}
		}

		return cache;
	}

	/// <summary>
	/// The tarkov.dev maps a maps.json entry applies to.
	///
	/// The two sources do not always name a map the same way. maps.json has one
	/// entry per artwork, while the API has a separate map per variant, so
	/// "ground-zero" is the artwork behind "ground-zero", "ground-zero-21" and
	/// "ground-zero-tutorial". Matching on equality alone leaves the variants
	/// with no map data at all, so they silently cannot be opened.
	///
	/// An exact match and any longer names sharing its prefix are both returned,
	/// since the artwork legitimately covers all of them. Order does not matter
	/// here: every hit maps to the same artwork.
	/// </summary>
	private static IEnumerable<TarkovDev.Json.Map> ResolveTarkovMaps(List<TarkovDev.Json.Map> tarkovMaps, string normalizedName) {
		// Guarded so a short or near-empty name cannot swallow most of the map
		// list; every real artwork name is well past this length.
		if (normalizedName.Length < MinPrefixLength) return [];

		return tarkovMaps.Where(m =>
			m.NormalizedName != null &&
			m.NormalizedName.StartsWith(normalizedName, StringComparison.Ordinal));
	}

	/// <summary>
	/// Shortest maps.json name allowed to match by prefix, to keep a loosely
	/// named entry from matching every map that happens to start the same way.
	/// </summary>
	private const int MinPrefixLength = 4;

	/// <summary>
	/// Gets the dictionary mapping map IDs to InteractiveMapData
	/// </summary>
	public static Dictionary<string, Map> GetMapsById() {
		if (_mapsByIdCache != null) return _mapsByIdCache;

		// Trigger loading if not already loaded
		_ = GetMapsData();

		return _mapsByIdCache ?? [];
	}

	/// <summary>
	/// Gets the SVG URL for a given map by its ID or normalized name
	/// </summary>
	public static string? GetMapSvgUrl(string? mapId) {
		if (string.IsNullOrEmpty(mapId)) {
			Logger.LogWarning($"GetMapSvgUrl called with null or empty mapId");
			return null;
		}

		var mapsById = GetMapsById();

		if (!mapsById.TryGetValue(mapId, out var mapData)) {
			Logger.LogWarning($"No map data found for ID: {mapId}");
			return null;
		}

		return mapData?.SvgPath;
	}

	private static readonly Dictionary<string, string?> _svgContentCache = [];

	/// <summary>
	/// Whether a map has its own banner art on disk. Banners are named after the
	/// map id, matching the SVG maps, so the file is looked up directly rather
	/// than through the projection data.
	/// </summary>
	public static bool MapBannerExists(string? mapId) {
		if (string.IsNullOrEmpty(mapId)) return false;
		return File.Exists(Path.Combine(RatConfig.Paths.Data, "banner", $"{mapId}.png"));
	}

	/// <summary>
	/// Whether the shared fallback banner is available, used for maps that have
	/// no art of their own.
	/// </summary>
	public static bool DefaultBannerExists => File.Exists(Path.Combine(RatConfig.Paths.Data, "banner", "default.png"));

	/// <summary>Shared for SVG fetches, which are immutable and cached below.</summary>
	private static readonly HttpClient SvgHttpClient = new();

	/// <summary>
	/// Raw SVG markup for a map, or null when it has none or the fetch failed.
	/// Cached per map since the maps are immutable and the viewer re-requests
	/// this on every parameter set.
	/// </summary>
	public static string? GetMapSvgContent(string? mapId) {
		if (string.IsNullOrEmpty(mapId)) return null;

		if (_svgContentCache.TryGetValue(mapId, out var cached)) return cached;

		string? content = null;
		try {
			var url = GetMapSvgUrl(mapId);
			if (!string.IsNullOrEmpty(url)) {
				content = SvgHttpClient.GetStringAsync(url).GetAwaiter().GetResult();
			}
		} catch (Exception e) {
			Logger.LogWarning($"Failed to fetch SVG for map {mapId}", e);
			content = null;
		}

		_svgContentCache[mapId] = content;
		return content;
	}

	/// <summary>
	/// The intrinsic width/height of a map's SVG, parsed from its viewBox.
	/// Callers use it to give a canvas the same aspect ratio as the artwork, so
	/// percentage-positioned overlays land on the map rather than the frame.
	/// </summary>
	public static bool TryGetSvgViewBoxSize(string? mapId, out double width, out double height) {
		width = 0;
		height = 0;

		var content = GetMapSvgContent(mapId);
		if (string.IsNullOrEmpty(content)) return false;

		// The markup is already cached, so re-parsing it is cheap and this avoids
		// a second cache that could fall out of step with the first.
		var match = ViewBoxRegex.Match(content);
		if (!match.Success) return false;

		return double.TryParse(match.Groups[3].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out width)
			&& double.TryParse(match.Groups[4].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out height)
			&& width > 0
			&& height > 0;
	}

	/// <summary>viewBox="minX minY width height"</summary>
	private static readonly Regex ViewBoxRegex = new(@"viewBox\s*=\s*[""']\s*([-\d.eE]+)\s+([-\d.eE]+)\s+([\d.eE]+)\s+([\d.eE]+)\s*[""']",
		RegexOptions.IgnoreCase | RegexOptions.Compiled);
}
