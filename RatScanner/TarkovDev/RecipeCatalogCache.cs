using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RatScanner.TarkovDev;

public sealed record RecipeSource(RecipeCatalog? Catalog, bool Stale) {
	public bool Available => Catalog != null;
}

public sealed record RecipeSources(RecipeSource Crafts, RecipeSource Barters);

/// <summary>Optional recipe data: bounded per-mode cache, shared requests, and last-good offline fallback.</summary>
public sealed class RecipeCatalogCache(
	Func<GameMode, string, Task<string>> fetch,
	Func<string, string?> read,
	Action<string, string> write,
	Action<string, Exception> warn,
	TimeProvider? clock = null) {
	private sealed class Entry {
		public RecipeSource Value = new(null, false);
		public DateTimeOffset Expires;
		public Task<RecipeSource>? Pending;
	}

	private readonly object _gate = new();
	private readonly Dictionary<(GameMode, string), Entry> _entries = [];
	private readonly TimeProvider _clock = clock ?? TimeProvider.System;

	public async Task<RecipeSources> GetAsync(GameMode mode) {
		var crafts = GetSourceAsync(mode, "crafts");
		var barters = GetSourceAsync(mode, "barters");
		await Task.WhenAll(crafts, barters).ConfigureAwait(false);
		return new(await crafts.ConfigureAwait(false), await barters.ConfigureAwait(false));
	}

	private Task<RecipeSource> GetSourceAsync(GameMode mode, string endpoint) {
		lock (_gate) {
			if (!_entries.TryGetValue((mode, endpoint), out var entry)) {
				entry = new Entry();
				_entries.Add((mode, endpoint), entry);
			}
			if (entry.Pending != null) return entry.Pending;
			if (_clock.GetUtcNow() < entry.Expires) return Task.FromResult(entry.Value);
			// Start on the pool so neither disk reads nor a synchronous fetch can block rendering.
			entry.Pending = Task.Run(() => RefreshAsync(entry, mode, endpoint));
			return entry.Pending;
		}
	}

	private async Task<RecipeSource> RefreshAsync(Entry entry, GameMode mode, string endpoint) {
		var key = $"recipes_v1_{mode.ToApiString()}_{endpoint}";
		var value = entry.Value;
		var ttl = TimeSpan.FromMinutes(1);
		try {
			if (!value.Available) {
				try {
					var cached = read(key);
					if (cached != null) value = new(RecipeCatalog.Parse(cached, endpoint == "crafts"), true);
				} catch (Exception e) {
					warn($"Could not read cached {endpoint} for {mode}.", e);
				}
			}
			try {
				var response = await fetch(mode, endpoint).ConfigureAwait(false);
				value = new(RecipeCatalog.Parse(response, endpoint == "crafts"), false);
				ttl = TimeSpan.FromHours(12);
				try {
					write(key, response);
				} catch (Exception e) {
					// Persistence failure must not discard a valid network response.
					warn($"Could not save {endpoint} for {mode}.", e);
				}
			} catch (Exception e) {
				value = value with { Stale = value.Available };
				warn($"Could not refresh {endpoint} for {mode}.", e);
			}
			return value;
		} finally {
			lock (_gate) {
				entry.Value = value;
				entry.Expires = _clock.GetUtcNow() + ttl;
				entry.Pending = null;
			}
		}
	}
}
