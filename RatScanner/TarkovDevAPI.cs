using Newtonsoft.Json;
using RatScanner.TarkovDev.GraphQL;
using System;
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

	private static readonly Dictionary<string, (long expire, object response)> Cache = new();

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
		Logger.LogInfo("Queueing request for query: " + query[..32].ReplaceLineEndings(" "));

		Stopwatch swHttp = new();
		Stopwatch swJson = new();
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

		using Stream s = await Get(query);
		using StreamReader sr = new(s);
		using JsonReader reader = new JsonTextReader(sr);

		swJson.Start();
		ResponseData<T>? neededResponse = serializer.Deserialize<ResponseData<T>>(reader);
		if (neededResponse == null) throw new Exception("Failed to deserialize needed response");
		T? response = neededResponse.Data.Data;
		if (response == null) throw new Exception("Failed to deserialize response");

		// Update cache
		long time = DateTimeOffset.Now.ToUnixTimeSeconds();
		Cache[query] = (time + ttl, response);

		swHttp.Stop();
		swJson.Stop();
		Logger.LogInfo($"Refreshed cache in {swHttp.ElapsedMilliseconds}ms ({swJson.ElapsedMilliseconds}ms) for query: \"{query[..32].ReplaceLineEndings(" ")}\"");
	}

	private static T GetCached<T>(string query, long ttl) {
		if (!Cache.TryGetValue(query, out (long expire, object response) value)) {
			throw new Exception("Query not found in cache. Was InitializeCache called?");
		}

		// Queue request if cache is expired
		long time = DateTimeOffset.Now.ToUnixTimeSeconds();
		if (time > value.expire) Task.Run(() => QueueRequest<T>(query, ttl));

		return (T)value.response;
	}

	public static void InitializeCache() {
		Task.WhenAll(
			Task.Run(() => QueueRequest<TTask[]>(TasksQuery, 60 * 60 * 12)),
			Task.Run(() => QueueRequest<Item[]>(ItemsQuery(RatConfig.NameScan.Language.ToTarkovDevType(), RatConfig.GameMode), 60 * 60)),
			Task.Run(() => QueueRequest<HideoutStation[]>(HideoutStationsQuery, 60 * 60 * 12))
		).Wait();
	}

	/// <summary>
	/// Refreshes every 12 hours
	/// </summary>
	public static TTask[] GetTasks() => GetCached<TTask[]>(TasksQuery, 60 * 60 * 12);

	/// <summary>
	/// Refreshes every hour
	/// </summary>
	public static Item[] GetItems(LanguageCode language, GameMode gameMode) => GetCached<Item[]>(ItemsQuery(language, gameMode), 60 * 60);

	/// <summary>
	/// Refreshes every 12 hours
	/// </summary>
	public static HideoutStation[] GetHideoutStations() => GetCached<HideoutStation[]>(HideoutStationsQuery, 60 * 60 * 12);

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
			.WithBartersUsing(new BarterQueryBuilder().WithId())
			.WithCraftsFor(new CraftQueryBuilder().WithId())
			.WithCraftsUsing(new CraftQueryBuilder().WithId())
			.WithTypes()
			, alias: "data", lang: language, gameMode: gameMode).Build();
	}

	const string TasksQuery = @"{
  data: tasks {
    id
    tarkovDataId
    name
    trader {
      id
      name
    }
    map {
      id
      name
    }
    wikiLink
    minPlayerLevel
    taskRequirements {
      task {
        id
        name
      }
      status
    }
    traderLevelRequirements {
      trader {
        id
        name
      }
      level
    }
    objectives {
      __typename
      id
      type
      description
      optional
      maps {
        id
        name
      }
      ... on TaskObjectiveBuildItem {
        item {
          id
        }
        containsAll {
          id
        }
        containsOne {
          id
        }
      }
      ... on TaskObjectiveItem {
        item {
          id
        }
        count
        foundInRaid
        dogTagLevel
        maxDurability
        minDurability
      }
      ... on TaskObjectiveMark {
        markerItem {
          id
        }
      }
      ... on TaskObjectiveShoot {
        usingWeapon {
          id
        }
        usingWeaponMods {
          id
        }
        wearing {
          id
        }
        notWearing {
          id
        }
      }
      ... on TaskObjectiveQuestItem {
        id
        type
        description
        optional
        questItem {
          id
          name
          shortName
          description
        }
        count
      }
    }
    factionName
    neededKeys {
      keys {
        id
      }
      map {
        id
        name
      }
    }
  }
}
";

	const string HideoutStationsQuery = @"{
  data: hideoutStations {
    id
    name
    normalizedName
    levels {
      id
      level
      itemRequirements {
        id
        item {
          id
        }
        count
      }
      stationLevelRequirements {
        id
        station {
          id
          name
        }
        level
      }
      crafts {
        id
        duration
        requiredItems {
          item {
            id
          }
          count
        }
        rewardItems {
          item {
            id
          }
          count
        }
      }
    }
  }
}";
}
