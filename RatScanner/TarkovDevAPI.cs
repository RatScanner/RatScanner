using Newtonsoft.Json;
using RatScanner.TarkovDev.GraphQL;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace RatScanner;

/// <todo>
/// We absolutely want to move this into the main project at some point
/// </todo>
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

	private static string Get(string query) {
		Dictionary<string, string> body = new() { { "query", query } };
		System.Threading.Tasks.Task<HttpResponseMessage> responseTask = HttpClient.PostAsJsonAsync(ApiEndpoint, body);
		responseTask.Wait();

		if (responseTask.Result.StatusCode != HttpStatusCode.OK) throw new Exception("Tarkov.dev API request failed.");
		System.Threading.Tasks.Task<string> contentTask = responseTask.Result.Content.ReadAsStringAsync();
		contentTask.Wait();

		return contentTask.Result;
	}

	private static T GetCached<T>(string query, long ttl = 0xFFFFFF) {
		long time = DateTimeOffset.Now.ToUnixTimeSeconds();
		if (Cache.TryGetValue(query, out (long expire, object response) value) && time < value.expire) {
			return (T)value.response;
		}

		string apiResponse = Get(query);
		JsonSerializerSettings jsonSerializerSettings = new() {
			MissingMemberHandling = MissingMemberHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto,
			TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
		};

		ResponseData<T>? neededResponse = JsonConvert.DeserializeObject<ResponseData<T>>(apiResponse, jsonSerializerSettings);
		T? response = neededResponse.Data.Data;

		if (ttl > 0) Cache[query] = (time + ttl, response);

		return response;
	}

	public static Task[] GetTasks() => GetCached<Task[]>(TasksQuery);

	/// <summary>
	/// Refreshes every hour
	/// </summary>
	public static Item[] GetItems(LanguageCode language, GameMode gameMode) => GetCached<Item[]>(ItemsQuery(language, gameMode), 60 * 60);
	public static HideoutStation[] GetHideoutStations() => GetCached<HideoutStation[]>(HideoutStationsQuery);

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
