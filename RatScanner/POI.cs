using System.Collections.Generic;

namespace RatScanner;

/// <summary>
/// A single vertex of a POI's polygon, in the same percentage space as
/// <see cref="POI.X"/> and <see cref="POI.Y"/>.
/// </summary>
public class POIPoint {
    /// <summary>Percentage of the map's width (0-100).</summary>
    public double X { get; set; }

    /// <summary>Percentage of the map's height (0-100).</summary>
    public double Y { get; set; }
}

/// <summary>
/// A point of interest to draw on a map, e.g. an extraction, a trader, a
/// quest objective or a scav spawn.
/// </summary>
public class POI {
    /// <summary>
    /// Label shown under the icon. Not localized: callers own their own text.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SVG markup for the icon, inlined so it stays sharp at any zoom.
    /// An <img> would be rasterized once and blur when zoomed, same as the map
    /// itself. Falls back to a generic dot when empty.
    /// </summary>
    public string IconSvg { get; set; } = string.Empty;

    /// <summary>
    /// In-game X coordinate, as a percentage of the map's width (0-100).
    /// Percentages are used rather than raw map pixels because they stay
    /// correct across the different projections the map SVGs are drawn in.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// In-game Y coordinate, as a percentage of the map's height (0-100).
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// CSS color for the name label, e.g. "#ff9800". Defaults to the app's
    /// primary text color when empty.
    /// </summary>
    public string NameColor { get; set; } = string.Empty;

    /// <summary>
    /// CSS color for the marker's icon disc, e.g. "#4fc3f7". Defaults to amber
    /// when empty. Set this so each kind of marker is told apart at a glance on
    /// the map; the label color alone only affects the text.
    /// </summary>
    public string IconColor { get; set; } = string.Empty;

    /// <summary>
    /// CSS color for the icon disc while hovered. Falls back to
    /// <see cref="IconColor"/> when empty, so a marker does not lose its
    /// identity on hover.
    /// </summary>
    public string IconColorHover { get; set; } = string.Empty;

    /// <summary>
    /// Outline of the area this POI covers, as a series of vertices in the same
    /// percentage space as <see cref="X"/> and <see cref="Y"/>. Empty for the
    /// common case of a POI that is just a single spot.
    ///
    /// Percentages rather than world coordinates so the shape survives the
    /// per-map projection, exactly as the point position does. When set, the
    /// polygon is filled and stroked behind the icon, and the icon stays
    /// anchored at <see cref="X"/>/<see cref="Y"/>.
    /// </summary>
    public List<POIPoint> Points { get; set; } = new();

    /// <summary>
    /// Whether the polygon should be drawn filled and stroked rather than just
    /// stroked. Off by default so an outline-only zone does not obscure the
    /// map underneath it.
    /// </summary>
    public bool FillPolygon { get; set; }

    /// <summary>
    /// Stroke width of the polygon, in screen pixels. Drawn with a
    /// non-scaling stroke so it keeps the same on-screen weight at every zoom
    /// level instead of ballooning as the map is scaled up. Zero falls back to
    /// a default.
    /// </summary>
    public double StrokeWidth { get; set; }
}
