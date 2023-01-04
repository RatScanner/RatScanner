using Newtonsoft.Json;
using RatScanner.FetchModels.tarkovdata;
using RatScanner.FetchModels.tarkovdev;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace RatScanner;

/// <todo>
/// We absolutely want to move this into the main project at some point
/// </todo>
public class TarkovDevAPI
{
	const string ApiEndpoint = "https://api.tarkov.dev/graphql";

	private static readonly HttpClient httpClient = new(new HttpClientHandler
	{
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
	});

	private static HttpRequestMessage Request(HttpMethod method, string url)
	{
		var request = new HttpRequestMessage(method, url);
		request.Headers.Add("User-Agent", "RatScanner-Client/3");

		return request;
	}

	private static string Get(string query)
	{
		var body = new Dictionary<string, string>() { { "query", query } };
		var responseTask = httpClient.PostAsJsonAsync(ApiEndpoint, body);
		responseTask.Wait();
		if (responseTask.Result.StatusCode != HttpStatusCode.OK) throw new Exception("Tarkov.dev API request failed.");
		var contentTask = responseTask.Result.Content.ReadAsStringAsync();
		contentTask.Wait();

		return contentTask.Result;
	}
	
	public static List<NeededItem> GetNeededItems()
	{
		var apiResponse = Get(NeededQuery);
		var jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
		jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
		var neededResponse = JsonConvert.DeserializeObject<NeededResponse>(apiResponse, jsonSerializerSettings);
		return neededResponse.GetNeededItems();
	}

	const string NeededQuery = @"
	 query TarkovData {
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
			id
			type
			maps {
			  id
			  name
			}
			optional
			__typename
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
