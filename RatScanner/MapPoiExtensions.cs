using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;

namespace RatScanner;

/// <summary>
/// Helpers for turning map data into things the map viewer can draw.
/// </summary>
public static class MapPoiExtensions {
    /// <summary>
    /// The map's extraction points as POIs, ready to hand to MapViewer.
    /// Returns an empty list for maps with no projection data or no extracts.
    /// </summary>
    public static List<POI> GetExtractPois(this Map map) {
        if (map.Extracts.Count == 0) return [];

        // World coordinates only mean something relative to a map's own
        // projection, so a map without one has nothing to place.
        var projection = MapDataLoader.GetMapsById().GetValueOrDefault(map.Id);
        if (projection == null || MapProjection.Create(projection) is not { } frame) return [];

        var pois = new List<POI>(map.Extracts.Count);

        foreach (var extract in map.Extracts) {
            if (extract.Position == null) continue;

            // Out-of-range marks are kept and clamped, matching tarkov.dev, so an
            // extract near the map edge stays visible instead of disappearing.
            var (x, y) = frame.ToPercent(extract.Position);

            pois.Add(new POI {
                Name = extract.Name,
                IconSvg = ExtractIcon,
                // Distinguish the always-available extracts (shared faction) from
                // the ones tied to a faction or a switch condition.
                NameColor = extract.Faction == "shared" ? "#66bb6a" : "#ff9800",
                X = x,
                Y = y,
            });
        }

        return pois;
    }

    /// <summary>
    /// A downward arrow into a doorway, used as the extraction marker.
    /// Inlined rather than loaded as a file so it needs no extra asset and
    /// stays sharp at any zoom.
    /// </summary>
    private const string ExtractIcon = """
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
		     stroke="#fff" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
			<path d="M12 3v12" />
			<path d="M7 11l5 5 5-5" />
			<path d="M5 21h14" />
		</svg>
		""";
}
