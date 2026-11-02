using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RatScanner.TarkovDev.Json;

public class RecipeItem {
	[JsonProperty("item")]
	public string ItemId { get; set; } = string.Empty;
	[JsonProperty("count")]
	public decimal Count { get; set; }
	[JsonProperty("attributes")]
	public Dictionary<string, JToken> Attributes { get; set; } = [];
}

public sealed class RecipeQuestItem : RecipeItem {
	// Quest-item requirements in the API omit quantity; possessing one is required.
	public RecipeQuestItem() => Count = 1;
}

public sealed class ItemRecipe {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;
	[JsonProperty("station")]
	public string? StationId { get; set; }
	[JsonProperty("trader")]
	public string? TraderId { get; set; }
	[JsonProperty("level")]
	public int StationLevel { get; set; }
	[JsonProperty("minTraderLevel")]
	public int TraderLevel { get; set; }
	[JsonProperty("duration")]
	public int DurationSeconds { get; set; }
	[JsonProperty("taskUnlock")]
	public string? TaskUnlockId { get; set; }
	[JsonProperty("gameEditions")]
	public string[] GameEditions { get; set; } = [];
	[JsonProperty("requiredItems")]
	public RecipeItem[] RequiredItems { get; set; } = [];
	[JsonProperty("requiredQuestItems")]
	public RecipeQuestItem[] RequiredQuestItems { get; set; } = [];
	[JsonProperty("productItem")]
	public RecipeItem? ProductItem { get; set; }
	[JsonProperty("offeredItem")]
	public RecipeItem? OfferedItem { get; set; }
	[JsonIgnore]
	public RecipeItem Product => ProductItem ?? OfferedItem!;
}

/// <summary>A validated endpoint snapshot, indexed in both directions once per refresh.</summary>
public sealed class RecipeCatalog {
	private readonly Dictionary<string, ItemRecipe[]> _products;
	private readonly Dictionary<string, ItemRecipe[]> _ingredients;
	public int Count { get; }

	private RecipeCatalog(ItemRecipe[] recipes) {
		Count = recipes.Length;
		_products = recipes.GroupBy(r => r.Product.ItemId, StringComparer.Ordinal)
			.ToDictionary(g => g.Key, g => g.OrderBy(r => r.Id, StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);
		_ingredients = recipes.SelectMany(r => r.RequiredItems.Concat(r.RequiredQuestItems)
			.Select(i => i.ItemId).Distinct(StringComparer.Ordinal).Select(id => (id, recipe: r)))
			.GroupBy(x => x.id, StringComparer.Ordinal)
			.ToDictionary(g => g.Key, g => g.Select(x => x.recipe).OrderBy(r => r.Id, StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);
	}

	public IReadOnlyList<ItemRecipe> Producing(string itemId) => _products.GetValueOrDefault(itemId) ?? [];
	public IReadOnlyList<ItemRecipe> Using(string itemId) => _ingredients.GetValueOrDefault(itemId) ?? [];

	public static RecipeCatalog Parse(string response, bool crafts) {
		var document = JObject.Parse(response);
		if (document["data"] is not JArray data) throw new JsonException("Recipe response must contain a data array.");
		var recipes = data.ToObject<ItemRecipe[]>() ?? throw new JsonException("Recipe data is missing.");
		var ids = new HashSet<string>(StringComparer.Ordinal);
		foreach (var recipe in recipes) {
			if (recipe == null || string.IsNullOrWhiteSpace(recipe.Id) || !ids.Add(recipe.Id)
				|| (crafts ? recipe.ProductItem == null || recipe.OfferedItem != null || string.IsNullOrWhiteSpace(recipe.StationId)
					: recipe.OfferedItem == null || recipe.ProductItem != null || string.IsNullOrWhiteSpace(recipe.TraderId))
				|| recipe.RequiredItems == null || recipe.RequiredQuestItems == null || recipe.GameEditions == null
				|| recipe.DurationSeconds < 0 || recipe.StationLevel < 0 || recipe.TraderLevel < 0) {
				throw new JsonException("Recipe response contains an invalid or duplicate recipe.");
			}
			foreach (var item in recipe.RequiredItems.Concat(recipe.RequiredQuestItems).Append(recipe.Product)) {
				if (item == null || string.IsNullOrWhiteSpace(item.ItemId) || item.Count <= 0 || item.Attributes == null) {
					throw new JsonException($"Recipe {recipe.Id} contains an invalid item or quantity ({item?.ItemId}, {item?.Count}).");
				}
			}
		}
		return new RecipeCatalog(recipes);
	}
}
