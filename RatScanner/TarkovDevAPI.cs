using Newtonsoft.Json;
using RatScanner.TarkovDev.GraphQL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
	const int BatchSize = 200;

	private static readonly ConcurrentDictionary<string, (long expire, object response)> Cache = new();
	private static readonly ConcurrentDictionary<string, bool> PendingRequests = new();

	private static readonly HttpClient HttpClient = new(new HttpClientHandler {
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
	});

	private static readonly JsonSerializerSettings JsonSettings = new() {
		MissingMemberHandling = MissingMemberHandling.Ignore,
		NullValueHandling = NullValueHandling.Ignore,
		TypeNameHandling = TypeNameHandling.Auto,
		TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
	};

	private static async Task<Stream> Get(string query) {
		Dictionary<string, string> body = new() { { "query", query } };
		HttpResponseMessage responseTask = await HttpClient.PostAsJsonAsync(ApiEndpoint, body);

		if (responseTask.StatusCode != HttpStatusCode.OK) throw new Exception($"Tarkov.dev API request failed. {responseTask.ReasonPhrase}");
		return await responseTask.Content.ReadAsStreamAsync();
	}

	/// <summary>
	/// Tries to load data from offline cache
	/// </summary>
	/// <returns>True if cache was loaded successfully</returns>
	private static bool TryLoadFromOfflineCache<T>(string baseQueryKey, long ttl) where T : class {
		if (Cache.ContainsKey(baseQueryKey)) return true;

		if (RatConfig.ReadFromCache(baseQueryKey, out string cachedResponse)) {
			try {
				ResponseData<T[]>? neededResponse = JsonConvert.DeserializeObject<ResponseData<T[]>?>(cachedResponse, JsonSettings);
				if (neededResponse?.Data?.Data != null) {
					long time = DateTimeOffset.Now.ToUnixTimeSeconds();
					// Use expired TTL so background refresh will be triggered
					Cache[baseQueryKey] = (time - 1, neededResponse.Data.Data);
					Logger.LogInfo($"Loaded {neededResponse.Data.Data.Length} items from offline cache for: \"{baseQueryKey}\"");
					return true;
				}
			} catch (Exception e) {
				Logger.LogWarning($"Failed to load offline cache for: \"{baseQueryKey}\"", e);
			}
		}
		return false;
	}

	/// <summary>
	/// Fetches paginated data in batches and combines results
	/// </summary>
	private static async Task QueuePaginatedRequest<T>(string baseQueryKey, Func<int, int, string> queryBuilder, long ttl) where T : class {
		// Check if request is already pending
		if (!PendingRequests.TryAdd(baseQueryKey, true)) {
			Logger.LogInfo($"Request already pending for: \"{baseQueryKey}\", skipping");
			return;
		}

		try {
			List<T> allResults = new();
			List<string> rawBatchResponses = new();
			int offset = 0;
			bool hasMore = true;

			Stopwatch sw = Stopwatch.StartNew();

			while (hasMore) {
				string query = queryBuilder(BatchSize, offset);
				Logger.LogInfo($"Fetching batch at offset {offset} for: \"{baseQueryKey}\"");

				using Stream stream = await Get(query);
				using StreamReader streamReader = new(stream);
				
				// Read raw response for caching
				string rawResponse = await streamReader.ReadToEndAsync();
				rawBatchResponses.Add(rawResponse);
				
				// Parse the response
				ResponseData<T[]>? neededResponse = JsonConvert.DeserializeObject<ResponseData<T[]>>(rawResponse, JsonSettings);
				if (neededResponse?.Data?.Data == null) throw new Exception("Failed to deserialize paginated response");

				T[] batchResults = neededResponse.Data.Data;
				allResults.AddRange(batchResults);

				Logger.LogInfo($"Fetched {batchResults.Length} items at offset {offset} for: \"{baseQueryKey}\"");

				// If we got less than BatchSize, we've reached the end
				hasMore = batchResults.Length >= BatchSize;
				offset += BatchSize;
			}

			// Store combined results in cache
			long time = DateTimeOffset.Now.ToUnixTimeSeconds();
			T[] finalResults = allResults.ToArray();
			Cache[baseQueryKey] = (time + ttl, finalResults);

			// Combine raw responses for cache - extract and merge the data arrays
			string combinedCacheJson = CombineRawResponses<T>(rawBatchResponses);
			RatConfig.WriteToCache(baseQueryKey, combinedCacheJson);

			Logger.LogInfo($"Completed paginated fetch in {sw.ElapsedMilliseconds}ms: {allResults.Count} total items for \"{baseQueryKey}\"");
		} catch (Exception e) {
			Logger.LogWarning($"Failed paginated request for: \"{baseQueryKey}\".", e);

			// If we have existing cached data, extend its TTL to prevent rapid retries
			if (Cache.TryGetValue(baseQueryKey, out var existingCache))
			{
				long time = DateTimeOffset.Now.ToUnixTimeSeconds();
				Cache[baseQueryKey] = (time + RatConfig.SuperShortTTL, existingCache.response);
				Logger.LogInfo($"Extended cache TTL for: \"{baseQueryKey}\" to prevent rapid retries");
				return;
			}

			// Try to load from offline cache
			if (RatConfig.ReadFromCache(baseQueryKey, out string cachedResponse)) {
				Logger.LogInfo($"Read from offline cache for: \"{baseQueryKey}\"");

				ResponseData<T[]>? neededResponse = JsonConvert.DeserializeObject<ResponseData<T[]>?>(cachedResponse, JsonSettings);
				if (neededResponse?.Data?.Data != null) {
					long time = DateTimeOffset.Now.ToUnixTimeSeconds();
					Cache[baseQueryKey] = (time + RatConfig.SuperShortTTL, neededResponse.Data.Data);
					return;
				}
			}

			if (!Cache.ContainsKey(baseQueryKey)) throw new Exception("Failed to fetch paginated query response and no cache available.");
		} finally {
			// Always remove from pending requests when done
			PendingRequests.TryRemove(baseQueryKey, out _);
		}
	}

	/// <summary>
	/// Combines multiple raw JSON responses into a single cached response, preserving __typename fields
	/// </summary>
	private static string CombineRawResponses<T>(List<string> rawResponses) where T : class {
		List<Newtonsoft.Json.Linq.JToken> allDataItems = new();

		foreach (string rawResponse in rawResponses) {
			var json = Newtonsoft.Json.Linq.JObject.Parse(rawResponse);
			var dataArray = json["data"]?["data"] as Newtonsoft.Json.Linq.JArray;
			if (dataArray != null) {
				allDataItems.AddRange(dataArray);
			}
		}

		// Create combined response structure
		var combined = new Newtonsoft.Json.Linq.JObject {
			["data"] = new Newtonsoft.Json.Linq.JObject {
				["data"] = new Newtonsoft.Json.Linq.JArray(allDataItems)
			}
		};

		return combined.ToString(Newtonsoft.Json.Formatting.None);
	}

	private static T[] GetCachedPaginated<T>(string baseQueryKey, Func<int, int, string> queryBuilder, long ttl, bool isRetry = false) where T : class {
		if (!Cache.TryGetValue(baseQueryKey, out (long expire, object response) value)) {
			if (isRetry) throw new Exception("Retrying to fetch paginated query response failed.");

			Logger.LogInfo($"Paginated query not found in cache: \"{baseQueryKey}\"");
			Task.Run(() => QueuePaginatedRequest<T>(baseQueryKey, queryBuilder, ttl)).Wait();
			return GetCachedPaginated<T>(baseQueryKey, queryBuilder, ttl, true);
		}

		// Queue request if cache is expired and no request is already pending
		long time = DateTimeOffset.Now.ToUnixTimeSeconds();
		if (time > value.expire && !PendingRequests.ContainsKey(baseQueryKey)) {
			Task.Run(() => QueuePaginatedRequest<T>(baseQueryKey, queryBuilder, ttl));
		}

		return (T[])value.response;
	}

	/// <summary>
	/// Initializes cache from offline storage first, then queues background refresh.
	/// Returns true if all caches were loaded from offline storage.
	/// </summary>
	public static bool TryInitializeCacheFromOffline() {
		Logger.LogInfo("Attempting to load API cache from offline storage...");
		
		bool itemsLoaded = TryLoadFromOfflineCache<Item>(ItemsQueryKey(), RatConfig.MediumTTL);
		bool tasksLoaded = TryLoadFromOfflineCache<TTask>(TasksQueryKey(), RatConfig.LongTTL);
		bool hideoutLoaded = TryLoadFromOfflineCache<HideoutStation>(HideoutStationsQueryKey(), RatConfig.LongTTL);
		bool mapsLoaded = TryLoadFromOfflineCache<Map>(MapsQueryKey(), RatConfig.LongTTL);

		bool allLoaded = itemsLoaded && tasksLoaded && hideoutLoaded && mapsLoaded;
		
		if (allLoaded) {
			Logger.LogInfo("All API caches loaded from offline storage");
		} else {
			Logger.LogWarning($"Offline cache status - Items: {itemsLoaded}, Tasks: {tasksLoaded}, Hideout: {hideoutLoaded}, Maps: {mapsLoaded}");
		}

		return allLoaded;
	}

	/// <summary>
	/// Full cache initialization - waits for all requests to complete
	/// </summary>
	public static async Task InitializeCache() {
		await Task.WhenAll(
			Task.Run(() => QueuePaginatedRequest<Item>(ItemsQueryKey(), ItemsQueryPaginated, RatConfig.MediumTTL)),
			Task.Run(() => QueuePaginatedRequest<TTask>(TasksQueryKey(), TasksQueryPaginated, RatConfig.LongTTL)),
			Task.Run(() => QueuePaginatedRequest<HideoutStation>(HideoutStationsQueryKey(), HideoutStationsQueryPaginated, RatConfig.LongTTL)),
			Task.Run(() => QueuePaginatedRequest<Map>(MapsQueryKey(), MapsQueryPaginated, RatConfig.LongTTL))
		).ConfigureAwait(false);
	}

	public static Item[] GetItems(LanguageCode language, GameMode gameMode) => GetCachedPaginated<Item>(ItemsQueryKey(language, gameMode), (limit, offset) => ItemsQueryPaginated(limit, offset, language, gameMode), RatConfig.MediumTTL);
	public static Item[] GetItems() => GetCachedPaginated<Item>(ItemsQueryKey(), ItemsQueryPaginated, RatConfig.MediumTTL);

	public static TTask[] GetTasks(LanguageCode language, GameMode gameMode) => GetCachedPaginated<TTask>(TasksQueryKey(language, gameMode), (limit, offset) => TasksQueryPaginated(limit, offset, language, gameMode), RatConfig.LongTTL);
	public static TTask[] GetTasks() => GetCachedPaginated<TTask>(TasksQueryKey(), TasksQueryPaginated, RatConfig.LongTTL);

	public static HideoutStation[] GetHideoutStations(LanguageCode language, GameMode gameMode) => GetCachedPaginated<HideoutStation>(HideoutStationsQueryKey(language, gameMode), (limit, offset) => HideoutStationsQueryPaginated(limit, offset, language, gameMode), RatConfig.LongTTL);
	public static HideoutStation[] GetHideoutStations() => GetCachedPaginated<HideoutStation>(HideoutStationsQueryKey(), HideoutStationsQueryPaginated, RatConfig.LongTTL);

	public static Map[] GetMaps(LanguageCode language, GameMode gameMode) => GetCachedPaginated<Map>(MapsQueryKey(language, gameMode), (limit, offset) => MapsQueryPaginated(limit, offset, language, gameMode), RatConfig.LongTTL);
	public static Map[] GetMaps() => GetCachedPaginated<Map>(MapsQueryKey(), MapsQueryPaginated, RatConfig.LongTTL);

	#region Items Query

	private static string ItemsQueryKey() => ItemsQueryKey(RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string ItemsQueryKey(LanguageCode language, GameMode gameMode) => $"items_{language}_{gameMode}";

	private static string ItemsQueryPaginated(int limit, int offset) => ItemsQueryPaginated(limit, offset, RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string ItemsQueryPaginated(int limit, int offset, LanguageCode language, GameMode gameMode) {
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
		.WithBartersUsing(new BarterQueryBuilder().WithId())
		.WithCraftsFor(new CraftQueryBuilder().WithId())
		.WithCraftsUsing(new CraftQueryBuilder().WithId())
		.WithTypes()
		, alias: "data", lang: language, gameMode: gameMode, limit: limit, offset: offset).Build();
	}

	#endregion

	#region Tasks Query

	private static string TasksQueryKey() => TasksQueryKey(RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string TasksQueryKey(LanguageCode language, GameMode gameMode) => $"tasks_{language}_{gameMode}";

	private static string TasksQueryPaginated(int limit, int offset) => TasksQueryPaginated(limit, offset, RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string TasksQueryPaginated(int limit, int offset, LanguageCode language, GameMode gameMode) {
		return new QueryQueryBuilder().WithTasks(new TaskQueryBuilder().WithAllScalarFields()
			.WithKappaRequired()
			.WithMap(new MapQueryBuilder().WithAllScalarFields())
			.WithTrader(new TraderQueryBuilder().WithAllScalarFields())
			.WithObjectives(new TaskObjectiveQueryBuilder().WithAllScalarFields()
				.WithTaskObjectiveBasicFragment(new TaskObjectiveBasicQueryBuilder().WithAllScalarFields()
					.WithZones(new TaskZoneQueryBuilder().WithMap(new MapQueryBuilder().WithId()).WithPosition(new MapPositionQueryBuilder().WithAllScalarFields())))

				.WithTaskObjectiveBuildItemFragment(new TaskObjectiveBuildItemQueryBuilder().WithAllScalarFields()
					.WithItem(new ItemQueryBuilder().WithAllScalarFields()))

				.WithTaskObjectiveExperienceFragment(new TaskObjectiveExperienceQueryBuilder().WithAllScalarFields())

				.WithTaskObjectiveExtractFragment(new TaskObjectiveExtractQueryBuilder().WithAllScalarFields())

				.WithTaskObjectiveItemFragment(new TaskObjectiveItemQueryBuilder().WithAllScalarFields()
					.WithZones(new TaskZoneQueryBuilder().WithMap(new MapQueryBuilder().WithId()).WithPosition(new MapPositionQueryBuilder().WithAllScalarFields()))
					.WithItems(new ItemQueryBuilder().WithAllScalarFields()))

				.WithTaskObjectiveMarkFragment(new TaskObjectiveMarkQueryBuilder().WithAllScalarFields()
					.WithZones(new TaskZoneQueryBuilder().WithMap(new MapQueryBuilder().WithId()).WithPosition(new MapPositionQueryBuilder().WithAllScalarFields()))
					.WithMarkerItem(new ItemQueryBuilder().WithAllScalarFields()))

				.WithTaskObjectivePlayerLevelFragment(new TaskObjectivePlayerLevelQueryBuilder().WithAllScalarFields())

				.WithTaskObjectiveQuestItemFragment(new TaskObjectiveQuestItemQueryBuilder().WithAllScalarFields()
					.WithZones(new TaskZoneQueryBuilder().WithMap(new MapQueryBuilder().WithId()).WithPosition(new MapPositionQueryBuilder().WithAllScalarFields()))
					.WithQuestItem(new QuestItemQueryBuilder().WithAllScalarFields()))

				.WithTaskObjectiveShootFragment(new TaskObjectiveShootQueryBuilder().WithAllScalarFields())

				.WithTaskObjectiveSkillFragment(new TaskObjectiveSkillQueryBuilder().WithAllScalarFields())

				.WithTaskObjectiveTaskStatusFragment(new TaskObjectiveTaskStatusQueryBuilder().WithAllScalarFields())

				.WithTaskObjectiveTraderLevelFragment(new TaskObjectiveTraderLevelQueryBuilder().WithAllScalarFields()
					.WithTrader(new TraderQueryBuilder().WithAllScalarFields()))

				.WithTaskObjectiveTraderStandingFragment(new TaskObjectiveTraderStandingQueryBuilder().WithAllScalarFields()
					.WithTrader(new TraderQueryBuilder().WithAllScalarFields()))

				.WithTaskObjectiveUseItemFragment(new TaskObjectiveUseItemQueryBuilder().WithAllScalarFields()
					.WithZones(new TaskZoneQueryBuilder().WithMap(new MapQueryBuilder().WithId()).WithPosition(new MapPositionQueryBuilder().WithAllScalarFields()))
					.WithUseAny(new ItemQueryBuilder().WithAllScalarFields())))
			.WithTaskRequirements(new TaskStatusRequirementQueryBuilder().WithAllScalarFields()
				.WithTask(new TaskQueryBuilder().WithAllScalarFields()))
		, alias: "data", lang: language, gameMode: gameMode, limit: limit, offset: offset).Build();
	}

	#endregion

	#region HideoutStations Query

	private static string HideoutStationsQueryKey() => HideoutStationsQueryKey(RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string HideoutStationsQueryKey(LanguageCode language, GameMode gameMode) => $"hideout_{language}_{gameMode}";

	private static string HideoutStationsQueryPaginated(int limit, int offset) => HideoutStationsQueryPaginated(limit, offset, RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string HideoutStationsQueryPaginated(int limit, int offset, LanguageCode language, GameMode gameMode) {
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
		, alias: "data", lang: language, gameMode: gameMode, limit: limit, offset: offset).Build();
	}

	#endregion

	#region Maps Query

	private static string MapsQueryKey() => MapsQueryKey(RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string MapsQueryKey(LanguageCode language, GameMode gameMode) => $"maps_{language}_{gameMode}";

	private static string MapsQueryPaginated(int limit, int offset) => MapsQueryPaginated(limit, offset, RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode);
	private static string MapsQueryPaginated(int limit, int offset, LanguageCode language, GameMode gameMode)
	{
		return new QueryQueryBuilder().WithMaps(new MapQueryBuilder().WithAllScalarFields()
			.WithExtracts(new MapExtractQueryBuilder().WithAllScalarFields()
				.WithPosition(new MapPositionQueryBuilder().WithAllScalarFields())
				.WithTransferItem(new ContainedItemQueryBuilder().WithAllScalarFields()
					.WithItem(new ItemQueryBuilder().WithId())))
			.WithTransits(new MapTransitQueryBuilder().WithAllScalarFields()
				.WithPosition(new MapPositionQueryBuilder().WithAllScalarFields()))
		, alias: "data", lang: language, gameMode: gameMode, limit: limit, offset: offset).Build();
	}

	#endregion
}
