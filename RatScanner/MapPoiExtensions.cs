using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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
                IconColor = extract.Faction == "shared" ? "#2e7d32" : "#e65100",
                IconColorHover = extract.Faction == "shared" ? "#66bb6a" : "#ff9800",
                X = x,
                Y = y,
            });
        }

        return pois;
    }

    /// <summary>
    /// The map's keyed doors and trunks as POIs, labelled with the key they
    /// need. Returns an empty list for maps with no projection data or no locks.
    /// </summary>
    public static List<POI> GetLockPois(this Map map, Func<string, string?>? keyNameResolver = null) {
        // Only locks that actually need a key are worth showing. A lock with an
        // empty key is just a door, and marking every one would bury the map.
        var locks = map.Locks;
        if (locks.Count == 0) return [];

        var projection = MapDataLoader.GetMapsById().GetValueOrDefault(map.Id);
        if (projection == null || MapProjection.Create(projection) is not { } frame) return [];

        var pois = new List<POI>(locks.Count);

        foreach (var item in locks) {
            if (string.IsNullOrWhiteSpace(item.Key)) continue;

            var (x, y) = frame.ToPercent(item.Position);
            var keyName = keyNameResolver?.Invoke(item.Key);

            pois.Add(new POI {
                // Fall back to the raw key id so the mark is still identifiable
                // when the items endpoint has not loaded (or has no name for it).
                Name = keyName ?? item.Key,
                IconSvg = LockIcon,
                // Trunks are containers rather than doors, so tint them apart.
                NameColor = item.LockType == "trunk" ? "#ce93d8" : "#ffd54f",
                IconColor = item.LockType == "trunk" ? "#6a1b9a" : "#ff8f00",
                IconColorHover = item.LockType == "trunk" ? "#ce93d8" : "#ffd54f",
                X = x,
                Y = y,
            });
        }

        return pois;
    }

    /// <summary>
    /// The map's inter-map passages as POIs, labelled with where they lead.
    /// </summary>
    public static List<POI> GetTransitPois(this Map map, Func<string, string?>? mapNameResolver = null) {
        if (map.Transits.Count == 0) return [];

        var projection = MapDataLoader.GetMapsById().GetValueOrDefault(map.Id);
        if (projection == null || MapProjection.Create(projection) is not { } frame) return [];

        var pois = new List<POI>(map.Transits.Count);

        foreach (var transit in map.Transits) {
            var (x, y) = frame.ToPercent(transit.Position);
            var destination = mapNameResolver?.Invoke(transit.Map);

            pois.Add(new POI {
                Name = string.IsNullOrWhiteSpace(destination) ? transit.Description : destination,
                IconSvg = TransitIcon,
                NameColor = "#4fc3f7",
                IconColor = "#0277bd",
                IconColorHover = "#4fc3f7",
                X = x,
                Y = y,
            });
        }

        return pois;
    }

    /// <summary>
    /// The map's environmental hazards as POIs, e.g. minefields and sniper zones.
    /// </summary>
    public static List<POI> GetHazardPois(this Map map) {
        if (map.Hazards.Count == 0) return [];

        var projection = MapDataLoader.GetMapsById().GetValueOrDefault(map.Id);
        if (projection == null || MapProjection.Create(projection) is not { } frame) return [];

        var pois = new List<POI>(map.Hazards.Count);

        foreach (var hazard in map.Hazards) {
            var (x, y) = frame.ToPercent(hazard.Position);

            pois.Add(new POI {
                Name = HazardName(hazard),
                IconSvg = HazardIcon(hazard.HazardType),
                NameColor = HazardColor(hazard.HazardType),
                IconColor = HazardIconColor(hazard.HazardType),
                IconColorHover = HazardColor(hazard.HazardType),
                X = x,
                Y = y,
            });
        }

        return pois;
    }

    /// <summary>
    /// Label for a hazard. The API sends a translation key such as
    /// "DamageType_Landmine"; fall back to the raw key when it has not been
    /// translated so the mark is never blank.
    /// </summary>
    private static string HazardName(Hazard hazard) => string.IsNullOrWhiteSpace(hazard.Name) ? hazard.HazardType : hazard.Name;

    /// <summary>
    /// Colour a hazard by kind. Minefields are the dangerous ones worth noticing,
    /// so they read hottest.
    /// </summary>
    private static string HazardColor(string hazardType) => hazardType switch {
        "minefield" => "#ff5252",
        "sniper" => "#ff8a65",
        _ => "#ffb74d",
    };

    /// <summary>
    /// Darker disc color for a hazard, paired with <see cref="HazardColor"/> as
    /// its hover. Kept darker than the label color so the white icon on top stays
    /// readable.
    /// </summary>
    private static string HazardIconColor(string hazardType) => hazardType switch {
        "minefield" => "#b71c1c",
        "sniper" => "#bf360c",
        _ => "#e65100",
    };

    /// <summary>
    /// Icon for a hazard. Minefields get a burst, sniper zones a crosshair, and
    /// anything unrecognised falls back to a warning triangle.
    /// </summary>
    private static string HazardIcon(string hazardType) => hazardType switch {
        "minefield" => HazardMinefieldIcon,
        "sniper" => HazardSniperIcon,
        _ => Constants.Icon.TaskObjective.Warning,
    };

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

    /// <summary>A padlock, for a keyed door or trunk. Material Design's "lock" glyph.</summary>
    private const string LockIcon = """
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
		     stroke="#fff" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
			<rect x="4" y="10.5" width="16" height="10.5" rx="2" />
			<path d="M7.5 10.5V7a4.5 4.5 0 0 1 9 0v3.5" />
		</svg>
		""";

    /// <summary>
    /// Two arrows passing through each other, for a passage to another map.
    /// Material Design's "swap-horizontal" glyph.
    /// </summary>
    private const string TransitIcon = """
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
		     stroke="#fff" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
			<path d="M4 8h13l-3.5-3.5" />
			<path d="M20 16H7l3.5 3.5" />
		</svg>
		""";

    /// <summary>A bursting mine, for a minefield hazard.</summary>
    private const string HazardMinefieldIcon = """
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
		     stroke="#fff" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
			<circle cx="12" cy="12" r="2.6" />
			<path d="M12 3.2v3M12 17.8v3M3.2 12h3M17.8 12h3M5.8 5.8l2.1 2.1M16.1 16.1l2.1 2.1M18.2 5.8l-2.1 2.1M7.9 16.1l-2.1 2.1" />
		</svg>
		""";

    /// <summary>A crosshair, for a sniper zone hazard.</summary>
    private const string HazardSniperIcon = """
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
		     stroke="#fff" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
			<circle cx="12" cy="12" r="7.5" />
			<path d="M12 1.8v4.2M12 18v4.2M1.8 12H6M18 12h4.2" />
		</svg>
		""";

    /// <summary>
    /// A quest objective's zones as POIs, ready to hand to MapViewer.
    ///
    /// Zones are only returned for the map the projection belongs to, so an
    /// objective spanning several maps yields a marker on each one. Every zone
    /// in the data carries both a centre and a rectangular outline; the outline
    /// becomes the polygon's vertices and the centre anchors the icon, which
    /// keeps the icon sitting inside its own area.
    ///
    /// Returns an empty list when the map has no projection, since world
    /// coordinates mean nothing without one.
    /// </summary>
    public static List<POI> GetZonePois(this Map map, IEnumerable<TaskZone> zones, string? label = null) {
        var projection = MapDataLoader.GetMapsById().GetValueOrDefault(map.Id);
        if (projection == null || MapProjection.Create(projection) is not { } frame) return [];

        var pois = new List<POI>();

        foreach (var zone in zones) {
            // A zone belongs to exactly one map. Zones for other maps must not be
            // drawn here, as their coordinates belong to a different frame.
            if (!string.Equals(zone?.Map, map.Id, StringComparison.OrdinalIgnoreCase)) continue;

            var hasOutline = zone.Outline is { Count: >= 3 };
            var hasPosition = zone.Position != null;

            // Neither a centre nor an outline means nothing to draw at all.
            if (!hasOutline && !hasPosition) continue;

            // Prefer the outline's own centre when there is no explicit position,
            // so a shape-only zone still gets an icon in the middle of itself.
            double x, y;
            if (hasPosition) {
                (x, y) = frame.ToPercent(new WorldPosition {
                    X = zone.Position.X,
                    Y = zone.Position.Y,
                    Z = zone.Position.Z,
                });
            } else {
                var vertices = zone.Outline!.Select(o => frame.ToPercent(new WorldPosition {
                    X = o.X,
                    Y = o.Y,
                    Z = o.Z,
                })).ToList();
                x = vertices.Average(v => v.X);
                y = vertices.Average(v => v.Y);
            }

            var poi = new POI {
                Name = label ?? string.Empty,
                IconSvg = ZoneIcon,
                X = x,
                Y = y,
                NameColor = ZoneNameColor,
                IconColor = ZoneColor,
                IconColorHover = ZoneColorHover,
            };

            if (hasOutline) {
                poi.Points = zone.Outline!.Select(o => frame.ToPercent(new WorldPosition {
                    X = o.X,
                    Y = o.Y,
                    Z = o.Z,
                })).Select(p => new POIPoint { X = p.X, Y = p.Y }).ToList();
                poi.FillPolygon = true;
            }

            pois.Add(poi);
        }

        return pois;
    }

    /// <summary>
    /// The player's position as a POI, pointing the way they are facing, or null
    /// when there is no position yet or the map cannot be projected onto.
    /// </summary>
    public static POI? GetPlayerPositionPoi(this Map map, WorldPosition position, double rotationDegrees) {
        var projection = MapDataLoader.GetMapsById().GetValueOrDefault(map.Id);
        if (projection == null || MapProjection.Create(projection) is not { } frame) return null;

        var (x, y) = frame.ToPercent(position);

        return new POI {
            IconSvg = PlayerIcon,
            X = x,
            Y = y,
            IconColor = PlayerColor,
            IconColorHover = PlayerColorHover,
            RotationDegrees = rotationDegrees - (projection.CoordinateRotation ?? 0),
        };
    }

    /// <summary>Quest objective zones are cyan, distinct from every other overlay.</summary>
    private const string ZoneColor = "#00838f";

    private const string ZoneColorHover = "#26c6da";

    private const string ZoneNameColor = "#4dd0e1";

    /// <summary>A target ring over a dashed square, for an objective area.</summary>
    private const string ZoneIcon = """
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
		     stroke="#fff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
			<path d="M9 3H4a1 1 0 0 0-1 1v5M15 3h5a1 1 0 0 1 1 1v5M9 21H4a1 1 0 0 1-1-1v-5M15 21h5a1 1 0 0 0 1-1v-5" />
			<circle cx="12" cy="12" r="3.2" />
		</svg>
		""";

    /// <summary>The player marker is red, so it reads as the one live position.</summary>
    private const string PlayerColor = "#e53935";

    private const string PlayerColorHover = "#ff5252";

    /// <summary>
    /// A cone pointing up, so a rotation of 0 degrees means facing north, which
    /// is what the yaw from the log reports.
    /// </summary>
    private const string PlayerIcon = """
		<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
		     stroke="#fff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
			<path d="M12 3 L19 20 L12 16 L5 20 Z" fill="#fff" />
		</svg>
		""";
}
