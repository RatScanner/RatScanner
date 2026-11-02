using Newtonsoft.Json.Linq;
using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RatScanner.TarkovDev;

public sealed record RecipeItemDisplay(string ItemId, string? Name, decimal Count, bool Tool, bool Functional, int? MinLevel, bool QuestItem);
public sealed record RecipeDisplay(string Id, string? Source, int Level, bool Craft, int DurationSeconds,
	string? UnlockQuest, string[] GameEditions, RecipeItemDisplay Product, RecipeItemDisplay[] Ingredients);
public sealed record RecipeGroup(string TitleKey, RecipeDisplay[] Recipes);

public sealed record ItemRecipeDetailsModel(RecipeSources Sources, RecipeGroup[] Groups) {
	public static ItemRecipeDetailsModel Create(RecipeSources sources, string itemId, IEnumerable<Item> items,
		IEnumerable<HideoutStation> stations, IEnumerable<Trader> traders, IEnumerable<TarkovTask> tasks) {
		var itemNames = items.DistinctBy(i => i.Id).ToDictionary(i => i.Id, i => i.Name, StringComparer.Ordinal);
		var stationNames = stations.DistinctBy(s => s.Id).ToDictionary(s => s.Id, s => s.Name, StringComparer.Ordinal);
		var traderNames = traders.DistinctBy(t => t.Id).ToDictionary(t => t.Id, t => t.Name, StringComparer.Ordinal);
		var taskNames = tasks.DistinctBy(t => t.Id).ToDictionary(t => t.Id, t => t.Name, StringComparer.Ordinal);

		RecipeItemDisplay MapItem(RecipeItem item, bool questItem = false) => new(item.ItemId,
			itemNames.GetValueOrDefault(item.ItemId), item.Count,
			item.Attributes.TryGetValue("tool", out var tool) && tool.Type == Newtonsoft.Json.Linq.JTokenType.Boolean && tool.Value<bool>(),
			item.Attributes.TryGetValue("functional", out var functional) && functional.Type == Newtonsoft.Json.Linq.JTokenType.Boolean && functional.Value<bool>(),
			item.Attributes.TryGetValue("minLevel", out var level) && level.Type == Newtonsoft.Json.Linq.JTokenType.Integer ? level.Value<int>() : null,
			questItem);

		RecipeDisplay Map(ItemRecipe recipe, bool craft) => new(recipe.Id,
			craft ? stationNames.GetValueOrDefault(recipe.StationId!) : traderNames.GetValueOrDefault(recipe.TraderId!),
			craft ? recipe.StationLevel : recipe.TraderLevel, craft, recipe.DurationSeconds,
			string.IsNullOrEmpty(recipe.TaskUnlockId) ? null : taskNames.GetValueOrDefault(recipe.TaskUnlockId) ?? string.Empty,
			recipe.GameEditions, MapItem(recipe.Product),
			[.. recipe.RequiredItems.Select(i => MapItem(i)).Concat(recipe.RequiredQuestItems.Select(i => MapItem(i, true)))]);

		RecipeDisplay[] MapAll(IReadOnlyList<ItemRecipe>? recipes, bool craft) => recipes == null ? []
			: [.. recipes.Select(r => Map(r, craft)).OrderBy(r => r.Source, StringComparer.Ordinal)
				.ThenBy(r => r.Level).ThenBy(r => r.Product.Name, StringComparer.Ordinal).ThenBy(r => r.Id, StringComparer.Ordinal)];

		return new(sources, [
			new("RecipeCraftItem", MapAll(sources.Crafts.Catalog?.Producing(itemId), true)),
			new("RecipeBarterItem", MapAll(sources.Barters.Catalog?.Producing(itemId), false)),
			new("RecipeUsedInCrafts", MapAll(sources.Crafts.Catalog?.Using(itemId), true)),
			new("RecipeUsedInBarters", MapAll(sources.Barters.Catalog?.Using(itemId), false)),
		]);
	}
}
