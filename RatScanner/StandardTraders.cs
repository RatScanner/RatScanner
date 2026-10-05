using System.Collections.Generic;
using System.Linq;

namespace RatScanner;

/// <summary>
/// Picks out the eight traders a player actually has standing with, from the
/// full tarkov.dev list. The same endpoint also publishes story NPCs and raider
/// contacts (Ref, Lightkeeper, the BTR Driver, Mr. Kerman, Taran, Voevoda,
/// Survivor, Radio station), none of which have loyalty to set.
/// </summary>
/// <remarks>
/// Decided from the loyalty ladder alone, on purpose. <c>Trader.Name</c> is
/// localized — the untranslated payload literally calls all sixteen of them
/// "Nickname", and the real names only arrive from a separate translation
/// endpoint — so anything matching on name empties the list in every language
/// but English.
/// </remarks>
internal static class StandardTraders {
    /// <summary>
    /// Loyalty levels a trader must publish to count.
    ///
    /// Verified against the live regular-mode payload: the eight real traders
    /// publish 4 levels each except Fence, which publishes 3. Everything else
    /// publishes 1. No real trader sits below 3, so this drops the story NPCs
    /// without touching a real one.
    /// </summary>
    private const int MinLoyaltyLevels = 3;

    /// <summary>
    /// Reputation the TOP loyalty level must cost.
    ///
    /// Level count alone is not enough: Ref (the Arena host) also publishes four
    /// levels, so a plain "has 4 levels" filter returns nine traders. Ref's whole
    /// ladder is trivially cheap — LL4 at 1.2, with LL2 already at 0.25 — while
    /// every real trader's top tier costs at least 5.8:
    ///
    ///   Prapor 7.9, Mechanic 7.6, Jaeger 7.3, Ragman 6.5,
    ///   Therapist 5.8, Skier 5.8, Peacekeeper 6.0, Fence 6.0
    ///
    /// So a top-tier cost below this figure is an NPC, not a trader with a real
    /// standing track. The margin is wide (5.8 against 1.2) precisely so a
    /// balance patch nudging one threshold cannot flip the result.
    /// </summary>
    private const double MinTopLevelReputation = 2.0;

    /// <summary>
    /// The standard traders, in the order they are shown.
    ///
    /// Sorted by the loyalty ladder's own top-tier cost, which is stable across
    /// patches and identical in every language. Sorting by <c>Name</c> instead
    /// would reshuffle the grid per UI language.
    /// </summary>
    internal static List<Trader> Resolve(IEnumerable<Trader> traders) =>
        [.. traders.Where(IsStandard).OrderByDescending(TopLevelReputation)];
    /// <summary>Whether this trader has a real loyalty track rather than a token one.</summary>
    private static bool IsStandard(Trader trader) {
        var levels = trader.Levels;
        if (levels == null || levels.Count < MinLoyaltyLevels) {
            return false;
        }

        return TopLevelReputation(trader) >= MinTopLevelReputation;
    }

    /// <summary>
    /// Reputation the trader's highest published loyalty level costs, or 0 when
    /// they publish none. Uses the max over the ladder rather than assuming the
    /// last entry is the top one, since Fence's ladder starts at level 0.
    /// </summary>
    private static double TopLevelReputation(Trader trader) {
        var levels = trader.Levels;
        if (levels == null) {
            return 0;
        }

        double highest = 0;
        foreach (var level in levels) {
            if (level != null && level.RequiredReputation > highest) {
                highest = level.RequiredReputation;
            }
        }

        return highest;
    }
}
