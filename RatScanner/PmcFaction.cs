namespace RatScanner;

/// <summary>
/// Which side the player fights for. TarkovTracker and tarkov.dev both spell these
/// as bare upper-case names, so they live here rather than as string literals
/// scattered across the gates that compare them.
/// </summary>
public static class PmcFaction {
    public const string Usec = "USEC";

    public const string Bear = "BEAR";

    /// <summary>The factions a profile may be set to, for pickers.</summary>
    public static readonly string[] All = [Usec, Bear];
}