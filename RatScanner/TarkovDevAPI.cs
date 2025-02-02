using Newtonsoft.Json;
using RatScanner.TarkovDev.GraphQL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using TTask = RatScanner.TarkovDev.GraphQL.Task;

namespace RatScanner;

public static class TarkovDevAPI {
	private class ResponseData<T> {
		[JsonProperty("data")]
		public ResponseDataInner<T> Data { get; set; }
	}

	private class ResponseDataInner<T> {
		[JsonProperty("data")]
		public T Data { get; set; }
	}

	const string ApiEndpoint = "https://api.tarkov.dev/graphql";

	private static readonly ConcurrentDictionary<string, (long expire, object response)> Cache = new();

	private static readonly HttpClient HttpClient = new(new HttpClientHandler {
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
	});

	private static async Task<Stream> Get(string query) {
		Dictionary<string, string> body = new() { { "query", query } };
		HttpResponseMessage responseTask = await HttpClient.PostAsJsonAsync(ApiEndpoint, body);

		if (responseTask.StatusCode != HttpStatusCode.OK) throw new Exception("Tarkov.dev API request failed.");
		return await responseTask.Content.ReadAsStreamAsync();
	}

	private static async Task QueueRequest<T>(string query, long ttl) {
		try {
			await QueueRequestInner<T>(query, ttl);
		} catch (Exception e) {
			Logger.LogWarning($"Failed to request query: \"{query[..32].ReplaceLineEndings(" ")}...\".", e);

			if (RatConfig.ReadFromCache(query, out string cachedResponse)) {
				Logger.LogInfo($"Read from offline cache for query: \"{query[..32].ReplaceLineEndings(" ")}...\"");

				JsonSerializerSettings jsonSerializerSettings = new() {
					MissingMemberHandling = MissingMemberHandling.Ignore,
					NullValueHandling = NullValueHandling.Ignore,
					TypeNameHandling = TypeNameHandling.Auto,
					TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
				};

				ResponseData<T>? neededResponse = JsonConvert.DeserializeObject<ResponseData<T>?>(cachedResponse, jsonSerializerSettings);
				if (neededResponse == null) throw new Exception("Failed to deserialize cached response");
				T? response = neededResponse.Data.Data;
				if (response == null) throw new Exception("Failed to deserialize response");

				long time = DateTimeOffset.Now.ToUnixTimeSeconds();
				Cache[query] = (time + RatConfig.SuperShortTTL, response);
			}

			if (!Cache.ContainsKey(query)) throw new Exception("Failed to fetch query response and no cache available.");
		}
	}

	private static async Task QueueRequestInner<T>(string query, long ttl) {
		Logger.LogInfo($"Queueing request for query: \"{query[..32].ReplaceLineEndings(" ")}...\"");
		Stopwatch swHttp = new();
		Stopwatch swJson = new();
		Stopwatch swCache = new();
		swHttp.Start();

		// Prevent multiple requests for the same query
		Cache[query] = (long.MaxValue, Cache.GetValueOrDefault(query).response);

		JsonSerializerSettings jsonSerializerSettings = new() {
			MissingMemberHandling = MissingMemberHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto,
			TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
		};
		JsonSerializer serializer = JsonSerializer.Create(jsonSerializerSettings);

		using Stream stream = await Get(query);
		using StreamReader streamReader = new(stream);
		using JsonReader reader = new JsonTextReader(streamReader);

		swJson.Start();
		ResponseData<T>? neededResponse = serializer.Deserialize<ResponseData<T>>(reader);
		if (neededResponse == null) throw new Exception("Failed to deserialize needed response");
		T? response = neededResponse.Data.Data;
		if (response == null) throw new Exception("Failed to deserialize response");

		// Update cache
		long time = DateTimeOffset.Now.ToUnixTimeSeconds();
		Cache[query] = (time + ttl, response);

		swCache.Start();
		stream.Position = 0;
		string responseString = streamReader.ReadToEnd();
		RatConfig.WriteToCache(query, responseString);
		Logger.LogInfo($"Refreshed cache in {swHttp.ElapsedMilliseconds}ms ({swJson.ElapsedMilliseconds}ms) [{swCache.ElapsedMilliseconds}ms] for query: \"{query[..32].ReplaceLineEndings(" ")}...\"");
	}

	private static T GetCached<T>(string query, long ttl, bool isRetry = false) {
		if (!Cache.TryGetValue(query, out (long expire, object response) value)) {
			if (isRetry) throw new Exception("Retrying to fetch query response failed.");

			Logger.LogInfo($"Query not found in cache. Query: \"{query[..32].ReplaceLineEndings(" ")}...\"");
			Task.Run(() => QueueRequest<T>(query, ttl)).Wait();
			return GetCached<T>(query, ttl, true);
		}

		// Queue request if cache is expired
		long time = DateTimeOffset.Now.ToUnixTimeSeconds();
		if (time > value.expire) Task.Run(() => QueueRequest<T>(query, ttl));

		return (T)value.response;
	}

	public static async Task InitializeCache() {
		await Task.WhenAll(
			Task.Run(() => QueueRequest<Item[]>(ItemsQuery(), RatConfig.MediumTTL)),
			Task.Run(() => QueueRequest<TTask[]>(TasksQuery(), RatConfig.LongTTL)),
			Task.Run(() => QueueRequest<HideoutStation[]>(HideoutStationsQuery(), RatConfig.LongTTL))
		).ConfigureAwait(false);
	}

	public static Item[] GetItems(LanguageCode language, GameMode gameMode) => GetCached<Item[]>(ItemsQuery(language, gameMode), RatConfig.MediumTTL);
	public static Item[] GetItems() => GetCached<Item[]>(ItemsQuery(), RatConfig.MediumTTL);

	public static TTask[] GetTasks(LanguageCode language, GameMode gameMode) => GetCached<TTask[]>(TasksQuery(language, gameMode), RatConfig.LongTTL);
	public static TTask[] GetTasks() => GetCached<TTask[]>(TasksQuery(), RatConfig.LongTTL);

	public static HideoutStation[] GetHideoutStations(LanguageCode language, GameMode gameMode) => GetCached<HideoutStation[]>(HideoutStationsQuery(language, gameMode), RatConfig.LongTTL);
	public static HideoutStation[] GetHideoutStations() => GetCached<HideoutStation[]>(HideoutStationsQuery(), RatConfig.LongTTL);

	private static string ItemsQuery() => ItemsQuery(RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string ItemsQuery(LanguageCode language, GameMode gameMode) {
		return new QueryQueryBuilder().WithItems(new ItemQueryBuilder().WithAllScalarFields()
		.WithProperties(new ItemPropertiesQueryBuilder().WithAllScalarFields()
			.WithItemPropertiesAmmoFragment(new ItemPropertiesAmmoQueryBuilder().WithAllScalarFields())
			.WithItemPropertiesFoodDrinkFragment(new ItemPropertiesFoodDrinkQueryBuilder().WithAllScalarFields()
				.WithStimEffects(new StimEffectQueryBuilder().WithAllScalarFields()))
			.WithItemPropertiesStimFragment(new ItemPropertiesStimQueryBuilder().WithAllScalarFields()
				.WithStimEffects(new StimEffectQueryBuilder().WithAllScalarFields()))
			.WithItemPropertiesMedicalItemFragment(new ItemPropertiesMedicalItemQueryBuilder().WithAllScalarFields())
			.WithItemPropertiesMedKitFragment(new ItemPropertiesMedKitQueryBuilder().WithAllScalarFields()))
		.WithSellFor(new ItemPriceQueryBuilder().WithAllScalarFields()
			.WithVendor(new VendorQueryBuilder().WithAllScalarFields()
				.WithTraderOfferFragment(new TraderOfferQueryBuilder().WithAllScalarFields()
					.WithTrader(new TraderQueryBuilder().WithAllScalarFields()))))
		.WithBuyFor(new ItemPriceQueryBuilder().WithAllScalarFields()
			.WithVendor(new VendorQueryBuilder().WithAllScalarFields()
				.WithTraderOfferFragment(new TraderOfferQueryBuilder().WithAllScalarFields()
					.WithTrader(new TraderQueryBuilder().WithAllScalarFields()))))
		.WithCategory(new ItemCategoryQueryBuilder().WithAllScalarFields())
		.WithCategories(new ItemCategoryQueryBuilder().WithAllScalarFields())
		.WithUsedInTasks(new TaskQueryBuilder().WithId())
		.WithReceivedFromTasks(new TaskQueryBuilder().WithId())
		.WithBartersFor(new BarterQueryBuilder().WithId())
		.WithBartersUsing(new BarterQueryBuilder().WithId()
			.WithLevel()
			.WithTrader(new TraderQueryBuilder().WithAllScalarFields())
			.WithRequiredItems(new ContainedItemQueryBuilder().WithCount().WithItem(new ItemQueryBuilder().WithName()))
			.WithRewardItems(new ContainedItemQueryBuilder().WithCount().WithItem(new ItemQueryBuilder().WithName())))
		.WithCraftsFor(new CraftQueryBuilder().WithId())
		.WithCraftsUsing(new CraftQueryBuilder()
			.WithId()
			.WithLevel()
			.WithStation(new HideoutStationQueryBuilder().WithName())
			.WithRequiredItems(new ContainedItemQueryBuilder().WithCount().WithItem(new ItemQueryBuilder().WithName()))
			.WithRewardItems(
				new ContainedItemQueryBuilder()
					.WithItem(new ItemQueryBuilder().WithName())
					.WithCount()))
		.WithTypes()
		, alias: "data", lang: language, gameMode: gameMode).Build();
	}

	private static string TasksQuery() => TasksQuery(RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string TasksQuery(LanguageCode language, GameMode gameMode) {
		return new QueryQueryBuilder().WithTasks(new TaskQueryBuilder().WithAllScalarFields()
			.WithMap(new MapQueryBuilder().WithAllScalarFields())
			.WithTrader(new TraderQueryBuilder().WithAllScalarFields())
			.WithObjectives(new TaskObjectiveQueryBuilder().WithAllScalarFields()
				.WithTaskObjectiveBasicFragment(new TaskObjectiveBasicQueryBuilder().WithAllScalarFields())
				.WithTaskObjectiveBuildItemFragment(new TaskObjectiveBuildItemQueryBuilder().WithAllScalarFields().WithItem(new ItemQueryBuilder().WithAllScalarFields()))
				.WithTaskObjectiveExperienceFragment(new TaskObjectiveExperienceQueryBuilder().WithAllScalarFields())
				.WithTaskObjectiveExtractFragment(new TaskObjectiveExtractQueryBuilder().WithAllScalarFields())
				.WithTaskObjectiveItemFragment(new TaskObjectiveItemQueryBuilder().WithAllScalarFields().WithItems(new ItemQueryBuilder().WithAllScalarFields()))
				.WithTaskObjectiveMarkFragment(new TaskObjectiveMarkQueryBuilder().WithAllScalarFields().WithMarkerItem(new ItemQueryBuilder().WithAllScalarFields()))
				.WithTaskObjectivePlayerLevelFragment(new TaskObjectivePlayerLevelQueryBuilder().WithAllScalarFields())
				.WithTaskObjectiveQuestItemFragment(new TaskObjectiveQuestItemQueryBuilder().WithAllScalarFields().WithQuestItem(new QuestItemQueryBuilder().WithAllScalarFields()))
				.WithTaskObjectiveShootFragment(new TaskObjectiveShootQueryBuilder().WithAllScalarFields())
				.WithTaskObjectiveSkillFragment(new TaskObjectiveSkillQueryBuilder().WithAllScalarFields())
				.WithTaskObjectiveTaskStatusFragment(new TaskObjectiveTaskStatusQueryBuilder().WithAllScalarFields())
				.WithTaskObjectiveTraderLevelFragment(new TaskObjectiveTraderLevelQueryBuilder().WithAllScalarFields().WithTrader(new TraderQueryBuilder().WithAllScalarFields()))
				.WithTaskObjectiveTraderStandingFragment(new TaskObjectiveTraderStandingQueryBuilder().WithAllScalarFields().WithTrader(new TraderQueryBuilder().WithAllScalarFields()))
				.WithTaskObjectiveUseItemFragment(new TaskObjectiveUseItemQueryBuilder().WithAllScalarFields().WithUseAny(new ItemQueryBuilder().WithAllScalarFields())))
			.WithTaskRequirements(new TaskStatusRequirementQueryBuilder().WithAllScalarFields()
				.WithTask(new TaskQueryBuilder().WithAllScalarFields()))
		, alias: "data", lang: language, gameMode: gameMode).Build();
	}

	private static string HideoutStationsQuery() => HideoutStationsQuery(RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string HideoutStationsQuery(LanguageCode language, GameMode gameMode) {
		return new QueryQueryBuilder().WithHideoutStations(new HideoutStationQueryBuilder().WithAllScalarFields()
			.WithLevels(new HideoutStationLevelQueryBuilder().WithAllScalarFields()
				.WithItemRequirements(new RequirementItemQueryBuilder().WithAllScalarFields()
					.WithItem(new ItemQueryBuilder().WithAllScalarFields()))
				.WithStationLevelRequirements(new RequirementHideoutStationLevelQueryBuilder().WithAllScalarFields()
					.WithStation(new HideoutStationQueryBuilder().WithAllScalarFields()))
				.WithCrafts(new CraftQueryBuilder().WithAllScalarFields()
					.WithRequiredItems(new ContainedItemQueryBuilder().WithAllScalarFields()
						.WithItem(new ItemQueryBuilder().WithAllScalarFields()))
					.WithRewardItems(new ContainedItemQueryBuilder().WithAllScalarFields()
						.WithItem(new ItemQueryBuilder().WithAllScalarFields()))))
		, alias: "data", lang: language, gameMode: gameMode).Build();
	}
}
