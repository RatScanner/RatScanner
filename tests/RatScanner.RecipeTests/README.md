# Item recipe checks

Run on Windows x64 with the .NET 10 SDK:

```powershell
dotnet run --project tests/RatScanner.RecipeTests -c Release
```

The dependency-free assertion runner exits nonzero on failure. It covers input/output
indexes, duplicate and malformed data, fractional quantities, recipe requirements,
shared requests, mode isolation, expiry, partial failure, and offline persistence.
The build workflow runs these deterministic checks before publishing.

To additionally validate the current public craft and barter documents for all three
game modes:

```powershell
dotnet run --project tests/RatScanner.RecipeTests -c Release -- --live
```

To inspect the production recipe component in a real WebView2 window with isolated
fixtures (no game, scanner assets, tracker account, or saved configuration required):

```powershell
dotnet run --project tests/RatScanner.RecipeTests -c Release -- --ui
```

Expand both product groups and check source levels, quantities, quest and edition
restrictions, tool and dogtag requirements. Switch to Ingredient to check both usage
groups and highlighted inputs. Empty, Unavailable, Cached, and Loading exercise the
other states. Check keyboard expansion and narrow window layouts. Recipe names and
item links use the same production Razor component and scoped stylesheet as the app.

Recipe data loads asynchronously when the full item card opens. Requests are shared
per game mode and endpoint, time out after 15 seconds, and refresh after 12 hours.
Failures retain the last valid snapshot, retry on the next request after one minute,
and never prevent scanning. Validated raw documents are cached separately by mode;
an unavailable endpoint is distinguished from a successful empty result.
