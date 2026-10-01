namespace RatScanner;

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
}
