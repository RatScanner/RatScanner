using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RatScanner.InteractiveMapData;

namespace RatScanner;

public static class MapDataLoader
{
	private static Dictionary<string, Map>? _mapsByIdCache;
	
	/// <summary>
	/// Loads and caches the maps.json data
	/// </summary>
	public static Dictionary<string, Map>? GetMapsData()
	{
		if (_mapsByIdCache != null) return _mapsByIdCache;
		
		try
		{
			string mapsJsonPath = Path.Combine(RatConfig.Paths.Data, "maps.json");
			if (!File.Exists(mapsJsonPath)) {
				Logger.LogError($"maps.json not found at {mapsJsonPath}");
				return new();
			}
			
			string json = File.ReadAllText(mapsJsonPath);
			var mapsData = JsonConvert.DeserializeObject<List<InteractiveMapData>>(json) ?? new List<InteractiveMapData>();
			
			// Build the map ID cache
			_mapsByIdCache = BuildMapIdCache(mapsData);
			
			Logger.LogInfo($"Loaded {_mapsByIdCache.Count} maps from maps.json");
			return _mapsByIdCache;
		}
		catch (Exception e)
		{
			Logger.LogError("Failed to load maps.json", e);
			return new();
		}
	}
	
	/// <summary>
	/// Builds a dictionary mapping map IDs to their InteractiveMapData
	/// </summary>
	private static Dictionary<string, Map> BuildMapIdCache(List<InteractiveMapData> mapsData)
	{
		Dictionary<string, Map> cache = new();
		var tarkovDevMaps = TarkovDevAPI.GetMaps();

		foreach (InteractiveMapData mapData in mapsData) {
			if (mapData.Maps == null) continue;

			foreach (Map map in mapData.Maps) {
				if (string.IsNullOrEmpty(map.Key)) continue;
				if (map.Projection != "interactive") continue;

				var tMap = tarkovDevMaps.FirstOrDefault(m => m.NormalizedName == mapData.NormalizedName);
				if (tMap == null) {
					Logger.LogWarning($"No TarkovDev map match for normalized name: {mapData.NormalizedName}");
					continue;
				}
				cache[tMap.Id] = map;
			}
		}
		
		return cache;
	}
	
	/// <summary>
	/// Gets the dictionary mapping map IDs to InteractiveMapData
	/// </summary>
	public static Dictionary<string, Map> GetMapsById()
	{
		if (_mapsByIdCache != null) return _mapsByIdCache;
		
		// Trigger loading if not already loaded
		GetMapsData();
		
		return _mapsByIdCache ?? [];
	}
	
	/// <summary>
	/// Gets the SVG URL for a given map by its ID or normalized name
	/// </summary>
	public static string? GetMapSvgUrl(string? mapId)
	{
		if (string.IsNullOrEmpty(mapId)) {
			Logger.LogWarning($"GetMapSvgUrl called with null or empty mapId");
			return null;
		}
		
		Dictionary<string, Map> mapsById = GetMapsById();
		
		if (!mapsById.TryGetValue(mapId, out Map? mapData)) {
			Logger.LogWarning($"No map data found for ID: {mapId}");
			return null;
		}
		
		return mapData?.SvgPath;
	}
}
