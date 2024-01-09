using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RatScanner.FetchModels.TarkovDev;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace RatScanner;

/// <todo>
/// We absolutely want to move this into the main project at some point
/// </todo>
public static class TarkovDevAPI
{
	private class ResponseData<T>
	{
		[JsonProperty("data")]
		public ResponseDataInner<T> Data { get; set; }
	}

	private class ResponseDataInner<T>
	{
		[JsonProperty("data")]
		public T Data { get; set; }
	}

	const string ApiEndpoint = "https://api.tarkov.dev/graphql";

	private static readonly Dictionary<string, (long expire, object response)> Cache = new();

	private static readonly HttpClient HttpClient = new(new HttpClientHandler
	{
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
	});

	private static string Get(string query)
	{
		var body = new Dictionary<string, string>() { { "query", query } };
		var responseTask = HttpClient.PostAsJsonAsync(ApiEndpoint, body);
		responseTask.Wait();

		if (responseTask.Result.StatusCode != HttpStatusCode.OK) throw new Exception("Tarkov.dev API request failed.");
		var contentTask = responseTask.Result.Content.ReadAsStringAsync();
		contentTask.Wait();

		var result = contentTask.Result;

		// Hack to allow json deserialization
		var replacement = @"""$type"":""RatScanner.FetchModels.TarkovDev.$1, RatScanner""$2";
		result = Regex.Replace(result, @"""__typename"":\s?""(.*?)""(,?)", replacement);

		return result;
	}

	public static (List<NeededItem> tasks, List<NeededItem> hideout) GetNeededItems(long ttl = 0xFFFFFF)
	{
		var time = DateTimeOffset.Now.ToUnixTimeSeconds();
		var key = nameof(GetNeededItems);
		if (Cache.ContainsKey(key) && time < Cache[key].expire)
		{
			return ((List<NeededItem> tasks, List<NeededItem> hideout))Cache[key].response;
		}

		var apiResponse = Get(NeededQuery);
		var jsonSerializerSettings = new JsonSerializerSettings()
		{
			MissingMemberHandling = MissingMemberHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto,
			TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
		};

		var neededResponse = JsonConvert.DeserializeObject<NeededResponse>(apiResponse, jsonSerializerSettings);
		var response = neededResponse.Data.GetNeededItems();

		if (ttl > 0) Cache[key] = (time + ttl, response);

		return response;
	}

	public static Item[] GetItems(long ttl = 0xFFFFFF)
	{
		var time = DateTimeOffset.Now.ToUnixTimeSeconds();
		var key = nameof(ItemQuery);
		if (Cache.ContainsKey(key) && time < Cache[key].expire)
		{
			return (Item[])Cache[key].response;
		}

		var apiResponse = Get(ItemQuery);
		var jsonSerializerSettings = new JsonSerializerSettings()
		{
			MissingMemberHandling = MissingMemberHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto,
			TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
		};
		System.IO.File.WriteAllText("test.json", apiResponse);

		var res = JsonConvert.DeserializeObject<ResponseData<Item[]>>(apiResponse, jsonSerializerSettings);
		var response = res.Data.Data;

		if (ttl > 0) Cache[key] = (time + ttl, response);

		return response;
	}

	const string ItemQuery = @"
{
  data: items {
    id
    name
    normalizedName
    shortName
    description
    basePrice
    updated
    width
    height
    backgroundColor
    iconLink
    gridImageLink
    baseImageLink
    inspectImageLink
    image512pxLink
    image8xLink
    wikiLink
    types
    avg24hPrice
    properties {
      __typename
      ... on ItemPropertiesAmmo {
        caliber
        stackMaxSize
        tracer
        tracerColor
        ammoType
        projectileCount
        damage
        armorDamage
        fragmentationChance
        ricochetChance
        penetrationChance
        penetrationPower
        accuracyModifier
        recoilModifier
        initialSpeed
        lightBleedModifier
        heavyBleedModifier
        durabilityBurnFactor
        heatFactor
        staminaBurnPerDamage
        ballisticCoeficient
        bulletDiameterMilimeters
        bulletMassGrams
      }
      ... on ItemPropertiesFoodDrink {
        energy
        hydration
        units
        stimEffects {
          type
          chance
          delay
          duration
          value
          percent
          skillName
          __typename
        }
      }
      ... on ItemPropertiesStim {
        useTime
        cures
        stimEffects {
          type
          chance
          delay
          duration
          value
          percent
          skillName
          __typename
        }
      }
      ... on ItemPropertiesMedicalItem {
        uses
        useTime
        cures
      }
      ... on ItemPropertiesMedKit {
        hitpoints
        useTime
        maxHealPerUse
        cures
        hpCostLightBleeding
        hpCostHeavyBleeding
      }
    }
    conflictingSlotIds
    accuracyModifier
    recoilModifier
    ergonomicsModifier
    hasGrid
    blocksHeadphones
    link
    lastLowPrice
    changeLast48h
    changeLast48hPercent
    low24hPrice
    high24hPrice
    lastOfferCount
    sellFor {
      vendor {
        __typename
        normalizedName
        ... on TraderOffer {
          trader {
            id
            imageLink
            normalizedName
          }
        }
      }
      priceRUB
    }
    buyFor {
      vendor {
        __typename
        normalizedName
        ... on TraderOffer {
          trader {
            id
            imageLink
            normalizedName
          }
        }
      }
      priceRUB
    }
    category {
      id
    }
    categories {
      id
    }
    bsgCategoryId
    weight
    velocity
    loudness
    bartersFor {
      id
      trader {
        id
      }
    }
    bartersUsing {
      id
    }
    craftsFor {
      id
    }
    craftsUsing {
      id
    }
  }
}
";

	const string NeededQuery = @"
query {
  tasks {
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
  hideoutStations {
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
}
";
}
