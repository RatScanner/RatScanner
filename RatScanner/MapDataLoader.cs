using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RatScanner;

public static class MapDataLoader
{
	private static List<InteractiveMapData>? _mapsData;
	private static Dictionary<string, InteractiveMapData>? _mapsByIdCache;
	
	/// <summary>
	/// Loads and caches the maps.json data
	/// </summary>
	public static List<InteractiveMapData> GetMapsData()
	{
		if (_mapsData != null) return _mapsData;
		
		try
		{
			string mapsJsonPath = Path.Combine(RatConfig.Paths.Data, "maps.json");
			if (!File.Exists(mapsJsonPath)) {
				Logger.LogWarning($"maps.json not found at {mapsJsonPath}");
				return new List<InteractiveMapData>();
			}
			
			string json = File.ReadAllText(mapsJsonPath);
			_mapsData = JsonConvert.DeserializeObject<List<InteractiveMapData>>(json) ?? new List<InteractiveMapData>();
			
			// Build the map ID cache
			_mapsByIdCache = BuildMapIdCache(_mapsData);
			
			Logger.LogInfo($"Loaded {_mapsData.Count} maps from maps.json");
			return _mapsData;
		}
		catch (Exception e)
		{
			Logger.LogWarning("Failed to load maps.json", e);
			return new List<InteractiveMapData>();
		}
	}
	
	/// <summary>
	/// Builds a dictionary mapping map IDs to their InteractiveMapData
	/// </summary>
	private static Dictionary<string, InteractiveMapData> BuildMapIdCache(List<InteractiveMapData> mapsData)
	{
		Dictionary<string, InteractiveMapData> cache = new(StringComparer.OrdinalIgnoreCase);
		
		foreach (InteractiveMapData mapData in mapsData) {
			if (mapData.Maps == null) continue;
			
			foreach (InteractiveMapData.Map map in mapData.Maps) {
				if (string.IsNullOrEmpty(map.Key)) continue;
				
				// Map the key (map ID) to the parent InteractiveMapData
				if (!cache.ContainsKey(map.Key)) {
					cache[map.Key] = mapData;
				}
			}
			
			// Also map the normalized name for backward compatibility
			if (!string.IsNullOrEmpty(mapData.NormalizedName) && !cache.ContainsKey(mapData.NormalizedName)) {
				cache[mapData.NormalizedName] = mapData;
			}
		}
		
		return cache;
	}
	
	/// <summary>
	/// Gets the dictionary mapping map IDs to InteractiveMapData
	/// </summary>
	public static Dictionary<string, InteractiveMapData> GetMapsById()
	{
		if (_mapsByIdCache != null) return _mapsByIdCache;
		
		// Trigger loading if not already loaded
		GetMapsData();
		
		return _mapsByIdCache ?? new Dictionary<string, InteractiveMapData>(StringComparer.OrdinalIgnoreCase);
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
		
		Dictionary<string, InteractiveMapData> mapsById = GetMapsById();
		
		if (!mapsById.TryGetValue(mapId, out InteractiveMapData? mapData)) {
			Logger.LogWarning($"No map data found for ID: {mapId}");
			return null;
		}
		
		// Find the first interactive map with an svgPath
		InteractiveMapData.Map? interactiveMap = mapData.Maps?
			.FirstOrDefault(m => m.Projection == "interactive" && !string.IsNullOrEmpty(m.SvgPath));
		
		return interactiveMap?.SvgPath;
	}
}
