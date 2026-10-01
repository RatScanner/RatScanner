namespace RatScanner;

/// <summary>
/// How much of a search result a result component should render. Each value maps
/// to a presentation context rather than a size, since the two views want
/// different amounts of detail from the same component.
/// </summary>
public enum DetailLevel {
    /// <summary>
    /// A bare row in the main UI search list: thumbnail and name only.
    /// </summary>
    SearchRow,

    /// <summary>
    /// An unselected row in the interactive overlay: compact, with the short name,
    /// quick links, progress chips and the trader avatar.
    /// </summary>
    OverlayRow,

    /// <summary>
    /// The selected row in the interactive overlay. Same compact row as
    /// <see cref="OverlayRow"/>, plus the outstanding objectives listed underneath
    /// it, so the overlay stays a to-do list. Quests only: the selected item uses
    /// <see cref="Full"/> instead.
    /// </summary>
    OverlayExpanded,

    /// <summary>
    /// The full card: pricing, requirements, prerequisites and the rest. Used by
    /// the main UI for the item or quest it is showing, and by the overlay for the
    /// selected item.
    /// </summary>
    Full,
}