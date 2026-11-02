using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using RatScanner.TarkovDev;
using RatScanner.TarkovDev.Json;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace RatScanner.RecipeTests;

internal static class Program {
	internal const string Crafts = """
		{"data":[{"id":"craft","station":"workbench","level":2,"duration":8200,"taskUnlock":"quest",
		"gameEditions":["edge_of_darkness"],"requiredQuestItems":[{"item":"quest-item"}],"requiredItems":[
		{"item":"ingredient","count":2,"attributes":{}},{"item":"tool","count":1,"attributes":{"tool":true}},
		{"item":"ingredient","count":1,"attributes":{"functional":true}}],
		"productItem":{"item":"product","count":3,"attributes":{}}}]}
		""";
	internal const string Barters = """
		{"data":[{"id":"barter","trader":"prapor","minTraderLevel":3,"taskUnlock":"quest",
		"requiredItems":[{"item":"ingredient","count":155.1,"attributes":{}},
		{"item":"dogtag","count":20,"attributes":{"minLevel":39}}],
		"offeredItem":{"item":"product","count":1,"attributes":{}}}]}
		""";

	[STAThread]
	private static int Main(string[] args) {
		if (args.Contains("--ui")) {
			RunUi();
			return 0;
		}
		try {
			RunTests().GetAwaiter().GetResult();
			if (args.Contains("--live")) VerifyLiveData().GetAwaiter().GetResult();
			return 0;
		} catch (Exception e) {
			Console.Error.WriteLine(e);
			return 1;
		}
	}

	internal static ItemRecipeDetailsModel Fixture(string itemId = "product", RecipeSources? sources = null) =>
		ItemRecipeDetailsModel.Create(sources ?? new(new(RecipeCatalog.Parse(Crafts, true), false), new(RecipeCatalog.Parse(Barters, false), false)), itemId,
		[
			new() { Id = "product", Name = "Crafted item <test>" }, new() { Id = "ingredient", Name = "Required ingredient" },
			new() { Id = "tool", Name = "Reusable tool" }, new() { Id = "dogtag", Name = "Dogtag" },
		], [new() { Id = "workbench", Name = "Workbench" }], [new() { Id = "prapor", Name = "Prapor" }],
		[new() { Id = "quest", Name = "Unlock quest" }]);

	private static async Task RunTests() {
		var passed = 0;
		void Check(bool condition, string message) {
			if (!condition) throw new InvalidOperationException(message);
			passed++;
		}
		var crafts = RecipeCatalog.Parse(Crafts, true);
		var barters = RecipeCatalog.Parse(Barters, false);
		Check(crafts.Producing("product").Count == 1 && crafts.Using("product").Count == 0, "Craft outputs and inputs must be distinct.");
		Check(crafts.Using("ingredient").Count == 1 && crafts.Using("tool").Count == 1, "Repeated ingredients must not duplicate recipes; tools must be indexed.");
		Check(barters.Producing("product").Count == 1 && barters.Using("ingredient").Count == 1, "Barters must index both directions.");
		Check(barters.Using("ingredient")[0].RequiredItems[0].Count == 155.1m, "Fractional quantities must survive parsing.");
		Check(crafts.Producing("PRODUCT").Count == 0 && crafts.Using("missing").Count == 0, "Lookups must use exact IDs and return empty for unknown items.");
		Check(RecipeCatalog.Parse("{\"data\":[]}", true).Count == 0, "An explicit empty response is valid.");
		foreach (var invalid in new[] { "{}", "{\"data\":null}", "{\"data\":{}}", "{\"data\":[null]}",
			Crafts.Replace("\"count\":3", "\"count\":0"), Crafts.Replace("\"count\":2", "\"count\":-1"),
			Crafts.Replace("\"station\":\"workbench\"", "\"station\":null"), Crafts.Replace("\"requiredItems\":[", "\"requiredItems\":null,\"ignored\":["),
			Crafts.Replace("\"productItem\"", "\"offeredItem\"") }) {
			try { _ = RecipeCatalog.Parse(invalid, true); throw new InvalidOperationException("Malformed recipe data was accepted."); } catch (Newtonsoft.Json.JsonException) { passed++; }
		}
		var duplicate = Newtonsoft.Json.Linq.JObject.Parse(Crafts);
		var array = (Newtonsoft.Json.Linq.JArray)duplicate["data"]!;
		array.Add(array[0].DeepClone());
		try { _ = RecipeCatalog.Parse(duplicate.ToString(), true); throw new InvalidOperationException("Duplicate IDs accepted."); } catch (Newtonsoft.Json.JsonException) { passed++; }
		var model = Fixture();
		Check(model.Groups[0].Recipes.Length == 1 && model.Groups[1].Recipes.Length == 1 && model.Groups[2].Recipes.Length == 0, "Display counts must match output recipes.");
		var displayedCraft = model.Groups[0].Recipes.Single();
		Check(displayedCraft.Source == "Workbench" && displayedCraft.Level == 2 && displayedCraft.UnlockQuest == "Unlock quest", "Station, level and quest names must resolve.");
		Check(displayedCraft.Ingredients[1].Tool && displayedCraft.Ingredients[2].Functional && displayedCraft.GameEditions.Single() == "edge_of_darkness", "Tools, functional requirements and editions must be preserved.");
		Check(displayedCraft.Ingredients[3].Count == 1 && displayedCraft.Ingredients[3].QuestItem && crafts.Using("quest-item").Count == 1, "Quest-item requirements omit quantity and must default to one.");
		Check(model.Groups[1].Recipes.Single().Ingredients[1].MinLevel == 39, "Dogtag minimum level must be preserved.");
		var ingredientModel = Fixture("ingredient");
		Check(ingredientModel.Groups[2].Recipes.Length == 1 && ingredientModel.Groups[3].Recipes.Length == 1 && ingredientModel.Groups[0].Recipes.Length == 0, "Input display must not be mistaken for acquisition.");
		var unnamed = ItemRecipeDetailsModel.Create(model.Sources, "product", [], [], [], []);
		Check(unnamed.Groups[0].Recipes[0].Source == null && unnamed.Groups[0].Recipes[0].Product.Name == null
			&& unnamed.Groups[0].Recipes[0].UnlockQuest == string.Empty, "Missing metadata must preserve recipes and allow localized fallback labels.");

		var clock = new TestClock();
		var calls = new ConcurrentDictionary<(GameMode, string), int>();
		var disk = new ConcurrentDictionary<string, string>();
		var errors = new ConcurrentQueue<string>();
		var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var fail = false;
		var failCraftsOnly = false;
		var cache = new RecipeCatalogCache(async (mode, endpoint) => {
			calls.AddOrUpdate((mode, endpoint), 1, (_, count) => count + 1);
			await release.Task;
			if (fail || (failCraftsOnly && endpoint == "crafts")) throw new HttpRequestException("Offline");
			return (endpoint == "crafts" ? Crafts : Barters).Replace("\"product\"", $"\"{mode}-product\"");
		}, key => disk.GetValueOrDefault(key), (key, value) => disk[key] = value, (message, _) => errors.Enqueue(message), clock);
		var first = cache.GetAsync(GameMode.Regular);
		var second = cache.GetAsync(GameMode.Regular);
		release.SetResult();
		await Task.WhenAll(first, second);
		Check(calls.Values.Sum() == 2, "Concurrent callers must share one request per endpoint.");
		Check(first.Result.Crafts.Available && first.Result.Barters.Available && disk.Count == 2, "Both responses must be cached on disk.");
		await cache.GetAsync(GameMode.Regular);
		Check(calls.Values.Sum() == 2, "Fresh results must be reused.");
		var pve = await cache.GetAsync(GameMode.Pve);
		Check(pve.Crafts.Catalog!.Producing("Pve-product").Count == 1 && pve.Crafts.Catalog.Producing("Regular-product").Count == 0, "Mode changes must not reuse another mode's recipes.");
		var regularAgain = await cache.GetAsync(GameMode.Regular);
		Check(regularAgain.Crafts.Catalog!.Producing("Regular-product").Count == 1 && calls.Values.Sum() == 4, "Switching back must reuse the correct mode.");
		clock.Advance(TimeSpan.FromHours(13));
		failCraftsOnly = true;
		var partial = await cache.GetAsync(GameMode.Regular);
		Check(partial.Crafts.Stale && !partial.Barters.Stale && partial.Crafts.Available, "A source failure must retain last-good data without tainting the other source.");
		var beforeRetry = calls.Values.Sum();
		await cache.GetAsync(GameMode.Regular);
		Check(calls.Values.Sum() == beforeRetry, "Failure retries must be throttled.");
		clock.Advance(TimeSpan.FromMinutes(2));
		failCraftsOnly = false;
		Check(!(await cache.GetAsync(GameMode.Regular)).Crafts.Stale, "An expired failure must recover on the next request.");
		fail = true;
		var cold = new RecipeCatalogCache((_, _) => throw new HttpRequestException("Offline"), key => disk.GetValueOrDefault(key), (_, _) => { }, (_, _) => { });
		var offline = await cold.GetAsync(GameMode.Regular);
		Check(offline.Crafts.Stale && offline.Barters.Stale && offline.Crafts.Available, "Cold offline startup must restore raw cached documents.");
		var missing = await cold.GetAsync(GameMode.PvpSeason);
		Check(!missing.Crafts.Available && !missing.Barters.Available, "Missing data must be unavailable, not a valid empty result.");
		var diskFailure = new RecipeCatalogCache((_, e) => Task.FromResult(e == "crafts" ? Crafts : Barters), _ => throw new IOException("Corrupt cache"), (_, _) => throw new IOException("Read-only disk"), (_, _) => { });
		var persisted = await diskFailure.GetAsync(GameMode.Regular);
		Check(persisted.Crafts.Available && !persisted.Crafts.Stale && persisted.Barters.Available, "Disk failures must not discard live data.");
		var malformed = new RecipeCatalogCache((_, e) => Task.FromResult(e == "crafts" ? "{}" : Barters), _ => null, (_, _) => { }, (_, _) => { });
		var invalidSource = await malformed.GetAsync(GameMode.Regular);
		Check(!invalidSource.Crafts.Available && invalidSource.Barters.Available, "Malformed endpoint data must fail independently.");

		var waiting = new Dictionary<GameMode, TaskCompletionSource<RecipeSources>> {
			[GameMode.Regular] = new(TaskCreationOptions.RunContinuationsAsynchronously),
			[GameMode.Pve] = new(TaskCreationOptions.RunContinuationsAsynchronously),
		};
		var selection = new ItemRecipeSelection(mode => waiting[mode].Task, (s, id, _) => Fixture(id, s), _ => { });
		var oldSelection = selection.LoadAsync("product", GameMode.Regular);
		var newSelection = selection.LoadAsync("ingredient", GameMode.Pve);
		waiting[GameMode.Regular].SetResult(new(new(crafts, false), new(barters, false)));
		await oldSelection;
		Check(selection.Loading && selection.Model == null, "An old mode completing must not replace or finish a newer selection.");
		waiting[GameMode.Pve].SetResult(new(new(crafts, false), new(barters, false)));
		await newSelection;
		Check(!selection.Loading && selection.Model!.Groups[2].Recipes.Length == 1, "The latest selection must own the displayed model.");
		var throws = new ItemRecipeSelection(_ => Task.FromResult(first.Result), (_, _, _) => throw new IOException("Metadata failed"), _ => { });
		await throws.LoadAsync("product", GameMode.Regular);
		Check(!throws.Loading && throws.Model != null && !throws.Model.Sources.Crafts.Available, "Presentation failures must clear loading and show an unavailable state.");
		var pendingDispose = new TaskCompletionSource<RecipeSources>(TaskCreationOptions.RunContinuationsAsynchronously);
		var disposed = new ItemRecipeSelection(_ => pendingDispose.Task, (s, id, _) => Fixture(id, s), _ => { });
		var pendingSelection = disposed.LoadAsync("product", GameMode.Regular);
		disposed.Clear();
		pendingDispose.SetResult(first.Result);
		await pendingSelection;
		Check(!disposed.Loading && disposed.Model == null, "Disposed cards must ignore shared in-flight requests.");
		Console.WriteLine($"PASS: {passed} recipe parsing, presentation and cache assertions.");
	}

	private static void RunUi() {
		var application = new Application();
		var services = new ServiceCollection();
		services.AddWpfBlazorWebView();
		services.AddMudServices();
		services.AddSingleton<LocalizationService>();
		var provider = services.BuildServiceProvider();
		var view = new BlazorWebView { HostPage = "wwwroot/recipe-tests.html", Services = provider };
		view.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(RecipeTestHost) });
		var window = new Window { Title = "Recipe UI validation", Width = 720, Height = 760, Content = view };
		application.Run(window);
		view.DisposeAsync().AsTask().GetAwaiter().GetResult();
		provider.Dispose();
	}

	private static async Task VerifyLiveData() {
		using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
		foreach (var mode in new[] { "regular", "pve", "pvp-season" }) {
			foreach (var endpoint in new[] { "crafts", "barters" }) {
				var response = await client.GetStringAsync($"https://json.tarkov.dev/{mode}/{endpoint}");
				var catalog = RecipeCatalog.Parse(response, endpoint == "crafts");
				if (catalog.Count == 0) throw new InvalidOperationException($"Live {mode}/{endpoint} returned no recipes.");
				Console.WriteLine($"PASS: {mode}/{endpoint}: {catalog.Count} valid recipes.");
			}
		}
	}

	private sealed class TestClock : TimeProvider {
		private DateTimeOffset _now = DateTimeOffset.UtcNow;
		public override DateTimeOffset GetUtcNow() => _now;
		public void Advance(TimeSpan duration) => _now += duration;
	}
}
