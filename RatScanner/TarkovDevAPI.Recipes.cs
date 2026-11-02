using RatScanner.TarkovDev;
using RatScanner.TarkovDev.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RatScanner;

public static partial class TarkovDevAPI {
	private static readonly RecipeCatalogCache Recipes = new(
		FetchRecipesAsync,
		key => RatConfig.ReadFromCache(key, out var value) ? value : null,
		RatConfig.WriteToCache,
		(message, error) => Logger.LogWarning(message, error));

	public static Task<RecipeSources> GetRecipesAsync(GameMode mode) => Recipes.GetAsync(mode);

	public static ItemRecipeDetailsModel GetRecipeDetails(RecipeSources sources, string itemId, GameMode mode) {
		// Metadata is already loaded by the main catalog. Never trigger synchronous HTTP from a render.
		var language = LanguageCode;
		return ItemRecipeDetailsModel.Create(sources, itemId,
			CachedRecipeMetadata<Item>(ItemsQueryKey(language, mode)),
			CachedRecipeMetadata<HideoutStation>(HideoutStationsQueryKey(language, mode)),
			CachedRecipeMetadata<Trader>(TradersQueryKey(language, mode)),
			CachedRecipeMetadata<TarkovTask>(TasksQueryKey(language, mode)));
	}

	private static T[] CachedRecipeMetadata<T>(string key) =>
		Cache.TryGetValue(key, out var entry) && entry.response is T[] items ? items : [];

	private static async Task<string> FetchRecipesAsync(GameMode mode, string endpoint) {
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
		return await Get(BuildUrl(mode.ToApiString(), endpoint), timeout.Token).ConfigureAwait(false);
	}
}
