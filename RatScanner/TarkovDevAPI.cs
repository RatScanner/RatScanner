using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RatScanner;

public static class TarkovDevAPI {
	private class ItemsResponse {
		[JsonProperty("items")]
		public Dictionary<string, Item> Items { get; set; } = new();
	}

	private class TasksResponse {
		[JsonProperty("tasks")]
		public Dictionary<string, TarkovTask> Tasks { get; set; } = new();
	}

	private class MapsResponse {
		[JsonProperty("maps")]
		public Dictionary<string, Map> Maps { get; set; } = new();
	}

	private class TradersResponse {
		[JsonProperty("traders")]
		public Dictionary<string, Trader> Traders { get; set; } = new();
	}

	const string ApiEndpoint = "https://json.tarkov.dev/";

	private static readonly ConcurrentDictionary<string, (long expire, object response)> Cache = new();
	private static readonly ConcurrentDictionary<string, bool> PendingRequests = new();

	private static readonly HttpClient HttpClient = new(new HttpClientHandler {
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
	});

	private static readonly JsonSerializerSettings JsonSettings = new() {
		MissingMemberHandling = MissingMemberHandling.Ignore,
		NullValueHandling = NullValueHandling.Ignore,
	};

	private static readonly JsonLoadSettings TranslationLoadSettings = new() {
		DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
	};

	private const string EndpointItems = "items";
	private const string EndpointTasks = "tasks";
	private const string EndpointHideout = "hideout";
	private const string EndpointMaps = "maps";
	private const string EndpointTraders = "traders";

	private static string LanguageCode => RatConfig.NameScan.Language.ToApiLanguageCode().ToLowerInvariant();

	private static string BuildUrl(string gameMode, string endpoint) {
		return $"{ApiEndpoint}{gameMode}/{endpoint}";
	}

	private static async Task<string> Get(string url) {
		using HttpRequestMessage request = new(HttpMethod.Get, url);
		request.Headers.UserAgent.ParseAdd($"RatScanner-Client/{RatConfig.Version}");

		HttpResponseMessage responseTask = await HttpClient.SendAsync(request);

		if (responseTask.StatusCode != HttpStatusCode.OK) throw new Exception($"Tarkov.dev API request failed. {responseTask.ReasonPhrase}");
		return await responseTask.Content.ReadAsStringAsync();
	}

	/// <summary>
	/// Tries to load data from offline cache
	/// </summary>
	/// <returns>True if cache was loaded successfully</returns>
	private static bool TryLoadFromOfflineCache<T>(string baseQueryKey, long ttl) where T : class {
		if (Cache.ContainsKey(baseQueryKey)) return true;

		if (RatConfig.ReadFromCache(baseQueryKey, out string cachedResponse)) {
			try {
				T[]? data = DeserializeArrayResponse<T>(cachedResponse);
				if (data != null) {
					long time = DateTimeOffset.Now.ToUnixTimeSeconds();
					// Use expired TTL so background refresh will be triggered
					Cache[baseQueryKey] = (time - 1, data);
					Logger.LogInfo($"Loaded {data.Length} items from offline cache for: \"{baseQueryKey}\"");
					return true;
				}
			} catch (Exception e) {
				Logger.LogWarning($"Failed to load offline cache for: \"{baseQueryKey}\"", e);
			}
		}
		return false;
	}

	/// <summary>
	/// Fetches a full endpoint, merges translations, and caches the resulting array
	/// </summary>
	private static async Task QueueEndpointRequest<T>(string baseQueryKey, string endpoint, Func<Dictionary<string, T>, T[]> transform, long ttl) where T : class {
		// Check if request is already pending
		if (!PendingRequests.TryAdd(baseQueryKey, true)) {
			Logger.LogInfo($"Request already pending for: \"{baseQueryKey}\", skipping");
			return;
		}

		try {
			Stopwatch sw = Stopwatch.StartNew();
			Logger.LogInfo($"Fetching endpoint for: \"{baseQueryKey}\"");

			string gameMode = RatConfig.GameMode.ToApiString();
			string baseUrl = BuildUrl(gameMode, endpoint);
			string rawResponse = await Get(baseUrl);
			Dictionary<string, T> data = DeserializeKeyedResponse<T>(endpoint, rawResponse);

			// Apply translations by requesting the localized variant when available
			if (TryFetchTranslations(endpoint, out Dictionary<string, string>? translations) && translations != null) {
				ApplyTranslations(endpoint, data, translations);
			}

			T[] finalResults = transform(data);
			long time = DateTimeOffset.Now.ToUnixTimeSeconds();
			Cache[baseQueryKey] = (time + ttl, finalResults);
			var options = new System.Text.Json.JsonSerializerOptions {
				IgnoreReadOnlyProperties = true
			};
			RatConfig.WriteToCache(baseQueryKey, System.Text.Json.JsonSerializer.Serialize(finalResults, options));

			Logger.LogInfo($"Completed fetch in {sw.ElapsedMilliseconds}ms: {finalResults.Length} total items for \"{baseQueryKey}\"");
		} catch (Exception e) {
			Logger.LogWarning($"Failed request for: \"{baseQueryKey}\".", e);

			// If we have existing cached data, extend its TTL to prevent rapid retries
			if (Cache.TryGetValue(baseQueryKey, out var existingCache)) {
				long time = DateTimeOffset.Now.ToUnixTimeSeconds();
				Cache[baseQueryKey] = (time + RatConfig.SuperShortTTL, existingCache.response);
				Logger.LogInfo($"Extended cache TTL for: \"{baseQueryKey}\" to prevent rapid retries");
				return;
			}

			// Try to load from offline cache
			if (RatConfig.ReadFromCache(baseQueryKey, out string cachedResponse)) {
				Logger.LogInfo($"Read from offline cache for: \"{baseQueryKey}\"");
				T[]? cachedData = DeserializeArrayResponse<T>(cachedResponse);
				if (cachedData != null) {
					long time = DateTimeOffset.Now.ToUnixTimeSeconds();
					Cache[baseQueryKey] = (time + RatConfig.SuperShortTTL, cachedData);
					return;
				}
			}

			if (!Cache.ContainsKey(baseQueryKey)) throw new Exception("Failed to fetch query response and no cache available.");
		} finally {
			// Always remove from pending requests when done
			PendingRequests.TryRemove(baseQueryKey, out _);
		}
	}

	private static T[]? DeserializeArrayResponse<T>(string rawResponse) where T : class {
		JObject json = JObject.Parse(rawResponse);
		JObject? data = json["data"] as JObject;
		if (data == null) return null;

		JsonSerializer serializer = JsonSerializer.Create(JsonSettings);
		List<T> results = new();

		foreach (JProperty property in data.Properties()) {
			if (property.Value is not JObject itemObject) continue;
			T? item = itemObject.ToObject<T>(serializer);
			if (item != null) results.Add(item);
		}

		return results.ToArray();
	}

	private static Dictionary<string, T> DeserializeKeyedResponse<T>(string endpoint, string rawResponse) where T : class {
		JObject json = JObject.Parse(rawResponse);
		JObject? data = json["data"] as JObject;
		if (data == null) throw new Exception($"Failed to deserialize {endpoint} response");

		JsonSerializer serializer = JsonSerializer.Create(JsonSettings);

		switch (endpoint) {
			case EndpointItems: {
				ItemsResponse? response = data.ToObject<ItemsResponse>(serializer);
				if (response == null) throw new Exception("Failed to deserialize items response");
				return response.Items.ToDictionary(p => p.Key, p => p.Value as T)!;
			}
			case EndpointTasks: {
				TasksResponse? response = data.ToObject<TasksResponse>(serializer);
				if (response == null) throw new Exception("Failed to deserialize tasks response");
				return response.Tasks.ToDictionary(p => p.Key, p => p.Value as T)!;
			}
			case EndpointMaps: {
				MapsResponse? response = data.ToObject<MapsResponse>(serializer);
				if (response == null) throw new Exception("Failed to deserialize maps response");
				return response.Maps.ToDictionary(p => p.Key, p => p.Value as T)!;
			}
			case EndpointHideout: {
				Dictionary<string, T>? response = data.ToObject<Dictionary<string, T>>(serializer);
				if (response == null) throw new Exception("Failed to deserialize hideout response");
				return response;
			}
			case EndpointTraders: {
					Dictionary<string, T>? response = data.ToObject<Dictionary<string, T>>(serializer);
					if (response == null) throw new Exception("Failed to deserialize traders response");
					return response;
				}

			default:
				throw new ArgumentException($"Unsupported endpoint: {endpoint}", nameof(endpoint));
		}
	}

	private static bool TryFetchTranslations(string endpoint, out Dictionary<string, string>? translations) {
		translations = null;
		string lang = LanguageCode;

		try {
			string gameMode = RatConfig.GameMode.ToApiString();
			string localizedUrl = BuildUrl(gameMode, $"{endpoint}_{lang}");
			string rawResponse = Get(localizedUrl).GetAwaiter().GetResult();
			JObject json = JObject.Parse(rawResponse, TranslationLoadSettings);
			JObject? data = json["data"] as JObject;
			if (data == null) return false;

			JsonSerializer serializer = JsonSerializer.Create(JsonSettings);
			translations = data.ToObject<Dictionary<string, string>>(serializer);
			return translations != null;
		} catch (Exception e) {
			Logger.LogWarning($"Failed to fetch translations for endpoint {endpoint}: {e.Message}");
			return false;
		}
	}

	private static void ApplyTranslations<T>(string endpoint, Dictionary<string, T> data, Dictionary<string, string> translations) where T : class {
		foreach (var pair in data) {
			switch (pair.Value) {
				case Item item:
					if (translations.TryGetValue($"{item.Id} Name", out string? name)) item.Name = name;
					if (translations.TryGetValue($"{item.Id} ShortName", out string? shortName)) item.ShortName = shortName;
					if (translations.TryGetValue($"{item.Id} Description", out string? description)) item.Description = description;
					break;
				case TarkovTask task:
					if (translations.TryGetValue($"{task.Id} name", out string? taskName)) task.Name = taskName;
					foreach (TaskObjective objective in task.Objectives) {
						if (translations.TryGetValue(objective.Id, out string? objectiveDescription)) objective.Description = objectiveDescription;
					}
					break;
				case Map map:
					if (translations.TryGetValue($"{map.Id} Name", out string? mapName)) map.Name = mapName;
					if (translations.TryGetValue($"{map.Id} Description", out string? mapDescription)) map.Description = mapDescription;
					break;
				case HideoutStation station:
					if (translations.TryGetValue(station.Name, out string? stationName)) station.Name = stationName;
					break;
				case Trader trader:
					if (translations.TryGetValue($"{trader.Id} Nickname", out string? traderName)) trader.Name = traderName;
					if (translations.TryGetValue($"{trader.Id} Description", out string? traderDescription)) trader.Description = traderDescription;
					break;
			}
		}
	}

	private static T[] GetCached<T>(string baseQueryKey, string endpoint, Func<Dictionary<string, T>, T[]> transform, long ttl, bool isRetry = false) where T : class {
		if (!Cache.TryGetValue(baseQueryKey, out (long expire, object response) value)) {
			if (isRetry) throw new Exception("Retrying to fetch query response failed.");

			Logger.LogInfo($"Query not found in cache: \"{baseQueryKey}\"");
			Task.Run(() => QueueEndpointRequest<T>(baseQueryKey, endpoint, transform, ttl)).Wait();
			return GetCached<T>(baseQueryKey, endpoint, transform, ttl, true);
		}

		// Queue request if cache is expired and no request is already pending
		long time = DateTimeOffset.Now.ToUnixTimeSeconds();
		if (time > value.expire && !PendingRequests.ContainsKey(baseQueryKey)) {
			Task.Run(() => QueueEndpointRequest<T>(baseQueryKey, endpoint, transform, ttl));
		}

		if (value.response == null) return Array.Empty<T>();
		return ((T[])value.response).Where(i => i != null).ToArray();
	}

	/// <summary>
	/// Initializes cache from offline storage first, then queues background refresh.
	/// Returns true if all caches were loaded from offline storage.
	/// </summary>
	public static bool TryInitializeCacheFromOffline() {
		Logger.LogInfo("Attempting to load API cache from offline storage...");

		bool itemsLoaded = TryLoadFromOfflineCache<Item>(ItemsQueryKey(), RatConfig.MediumTTL);
		bool tasksLoaded = TryLoadFromOfflineCache<TarkovTask>(TasksQueryKey(), RatConfig.LongTTL);
		bool hideoutLoaded = TryLoadFromOfflineCache<HideoutStation>(HideoutStationsQueryKey(), RatConfig.LongTTL);
		bool mapsLoaded = TryLoadFromOfflineCache<Map>(MapsQueryKey(), RatConfig.LongTTL);
		bool tradersLoaded = TryLoadFromOfflineCache<Trader>(TradersQueryKey(), RatConfig.LongTTL);

		bool allLoaded = itemsLoaded && tasksLoaded && hideoutLoaded && mapsLoaded && tradersLoaded;

		if (allLoaded) {
			Logger.LogInfo("All API caches loaded from offline storage");
		} else {
			Logger.LogWarning($"Offline cache status - Items: {itemsLoaded}, Tasks: {tasksLoaded}, Hideout: {hideoutLoaded}, Maps: {mapsLoaded}, Traders: {tradersLoaded}");
		}

		return allLoaded;
	}

	/// <summary>
	/// Full cache initialization - waits for all requests to complete
	/// </summary>
	public static async Task InitializeCache() {
		await Task.WhenAll(
			Task.Run(() => QueueEndpointRequest<Item>(ItemsQueryKey(), EndpointItems, d => d.Values.ToArray(), RatConfig.MediumTTL)),
			Task.Run(() => QueueEndpointRequest<TarkovTask>(TasksQueryKey(), EndpointTasks, d => d.Values.ToArray(), RatConfig.LongTTL)),
			Task.Run(() => QueueEndpointRequest<HideoutStation>(HideoutStationsQueryKey(), EndpointHideout, d => d.Values.ToArray(), RatConfig.LongTTL)),
			Task.Run(() => QueueEndpointRequest<Map>(MapsQueryKey(), EndpointMaps, d => d.Values.ToArray(), RatConfig.LongTTL)),
			Task.Run(() => QueueEndpointRequest<Trader>(TradersQueryKey(), EndpointTraders, d => d.Values.ToArray(), RatConfig.LongTTL))
		).ConfigureAwait(false);
		System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
	}

	public static Item[] GetItems(string language, GameMode gameMode) => GetCached<Item>(ItemsQueryKey(language, gameMode), EndpointItems, d => d.Values.ToArray(), RatConfig.MediumTTL);
	public static Item[] GetItems() => GetCached<Item>(ItemsQueryKey(), EndpointItems, d => d.Values.ToArray(), RatConfig.MediumTTL);

	public static TarkovTask[] GetTasks(string language, GameMode gameMode) => GetCached<TarkovTask>(TasksQueryKey(language, gameMode), EndpointTasks, d => d.Values.ToArray(), RatConfig.LongTTL);
	public static TarkovTask[] GetTasks() => GetCached<TarkovTask>(TasksQueryKey(), EndpointTasks, d => d.Values.ToArray(), RatConfig.LongTTL);

	public static HideoutStation[] GetHideoutStations(string language, GameMode gameMode) => GetCached<HideoutStation>(HideoutStationsQueryKey(language, gameMode), EndpointHideout, d => d.Values.ToArray(), RatConfig.LongTTL);
	public static HideoutStation[] GetHideoutStations() => GetCached<HideoutStation>(HideoutStationsQueryKey(), EndpointHideout, d => d.Values.ToArray(), RatConfig.LongTTL);

	public static Map[] GetMaps(string language, GameMode gameMode) => GetCached<Map>(MapsQueryKey(language, gameMode), EndpointMaps, d => d.Values.ToArray(), RatConfig.LongTTL);
	public static Map[] GetMaps() => GetCached<Map>(MapsQueryKey(), EndpointMaps, d => d.Values.ToArray(), RatConfig.LongTTL);

	public static Trader[] GetTraders(string language, GameMode gameMode) => GetCached<Trader>(TradersQueryKey(language, gameMode), EndpointTraders, d => d.Values.ToArray(), RatConfig.LongTTL);
	public static Trader[] GetTraders() => GetCached<Trader>(TradersQueryKey(), EndpointTraders, d => d.Values.ToArray(), RatConfig.LongTTL);

	#region Query Keys

	private static string ItemsQueryKey() => ItemsQueryKey(LanguageCode, RatConfig.GameMode);
	private static string ItemsQueryKey(string language, GameMode gameMode) => $"items_{language}_{gameMode}";

	private static string TasksQueryKey() => TasksQueryKey(LanguageCode, RatConfig.GameMode);
	private static string TasksQueryKey(string language, GameMode gameMode) => $"tasks_{language}_{gameMode}";

	private static string HideoutStationsQueryKey() => HideoutStationsQueryKey(LanguageCode, RatConfig.GameMode);
	private static string HideoutStationsQueryKey(string language, GameMode gameMode) => $"hideout_{language}_{gameMode}";

	private static string MapsQueryKey() => MapsQueryKey(LanguageCode, RatConfig.GameMode);
	private static string MapsQueryKey(string language, GameMode gameMode) => $"maps_{language}_{gameMode}";

	private static string TradersQueryKey() => TradersQueryKey(LanguageCode, RatConfig.GameMode);
	private static string TradersQueryKey(string language, GameMode gameMode) => $"traders_{language}_{gameMode}";

	#endregion
}
