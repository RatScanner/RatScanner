using System;
using System.Collections.Generic;
using static RatScanner.InteractiveMapData;

namespace RatScanner;

/// <summary>
/// Converts a game's 3D world coordinates into a percentage position on a map's
/// artwork.
///
/// Mirrors how tarkov.dev maps coordinates onto its SVG: the map's
/// <c>transform</c> and <c>coordinateRotation</c> project world coordinates into
/// the box the artwork is drawn into, and the result is expressed as a fraction
/// of that box. The artwork is drawn into the box described by
/// <c>svgBounds</c>, falling back to <c>bounds</c>, which is the frame these
/// percentages are relative to.
/// </summary>
internal readonly struct MapProjection {
    private readonly double _scaleX, _offsetX, _scaleZ, _offsetZ;
    private readonly double _cos, _sin;

    /// <summary>Left and right edges of the artwork, in projected units.</summary>
    private readonly double _left, _right;

    /// <summary>Top and bottom edges of the artwork, in projected units.</summary>
    private readonly double _top, _bottom;

    private MapProjection(double scaleX, double offsetX, double scaleZ, double offsetZ,
                          int rotation, double cornerAX, double cornerAY,
                          double cornerBX, double cornerBY) {
        _scaleX = scaleX;
        _offsetX = offsetX;
        _scaleZ = scaleZ;
        _offsetZ = offsetZ;

        var radians = rotation * Math.PI / 180;
        _cos = Math.Cos(radians);
        _sin = Math.Sin(radians);

        var a = Project(cornerAX, cornerAY);
        var b = Project(cornerBX, cornerBY);

        _left = Math.Min(a.X, b.X);
        _right = Math.Max(a.X, b.X);
        _top = Math.Min(a.Y, b.Y);
        _bottom = Math.Max(a.Y, b.Y);
    }

    /// <summary>
    /// Builds a projection for the map, or returns null when the map lacks the
    /// data needed to place anything on it.
    /// </summary>
    public static MapProjection? Create(Map map) {
        if (map.Transform is not { Count: >= 4 } transform) return null;
        if (transform[0] == null || transform[1] == null || transform[2] == null || transform[3] == null) return null;

        // The frame is the same two corners Leaflet bounds the map with, taken
        // through the projection. Each corner holds a longitude and a latitude
        // rather than two positions.
        var frame = map.SvgBounds is { Count: >= 2 } svgBounds ? svgBounds : map.Bounds;
        if (frame is not { Count: >= 2 } corners) return null;
        if (!IsUsable(corners[0]) || !IsUsable(corners[1])) return null;

        var projection = new MapProjection(
            transform[0]!.Value, transform[1]!.Value,
            transform[2]!.Value, transform[3]!.Value,
            map.CoordinateRotation ?? 0,
            corners[0][0]!.Value, corners[0][1]!.Value,
            corners[1][0]!.Value, corners[1][1]!.Value);

        // A degenerate frame would make every position come out as 0 or 100.
        if (projection._right <= projection._left || projection._bottom <= projection._top) return null;

        return projection;
    }

    /// <summary>
    /// The position as a percentage of the artwork, clamped to the frame.
    /// </summary>
    public (double X, double Y) ToPercent(TarkovDev.Json.WorldPosition position) {
        var (x, y) = Project(position.X, position.Z);
        return (Percent(x, _left, _right), Percent(y, _top, _bottom));
    }

    /// <summary>
    /// Rotates a world position by the map's coordinate rotation, then scales and
    /// offsets it into the artwork's box.
    /// </summary>
    private (double X, double Y) Project(double worldX, double worldZ) {
        var rotatedX = worldX * _cos - worldZ * _sin;
        var rotatedY = worldX * _sin + worldZ * _cos;

        // Horizontal comes from the rotated longitude, vertical from the rotated
        // latitude, and the vertical scale is negated because the artwork grows
        // downwards.
        return (_scaleX * rotatedX + _offsetX, -_scaleZ * rotatedY + _offsetZ);
    }

    private static bool IsUsable(List<double?> corner) {
        return corner is { Count: >= 2 } && corner[0] != null && corner[1] != null;
    }

    private static double Percent(double value, double min, double max) {
        return Math.Clamp((value - min) / (max - min) * 100, 0, 100);
    }
}
