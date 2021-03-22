﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RatScanner.FetchModels;

namespace RatScanner
{
	public static class ApiManager
	{
		public enum Language
		{
			English,
			Russian,
			German,
			French,
			Spanish,
			Chinese,
		}

		private static readonly Dictionary<Language, string> LanguageMapping = new Dictionary<Language, string>()
		{
			{Language.English, "en" },
			{Language.Russian, "ru" },
			{Language.German, "de" },
			{Language.French, "fr" },
			{Language.Spanish, "es" },
			{Language.Chinese, "cn" },
		};

		public enum ResourceType
		{
			ClientVersion,
			Download,
			Patreon,
			Github,
			Discord,
			FAQ,
		}

		private static readonly Dictionary<ResourceType, string> ResMapping = new Dictionary<ResourceType, string>
		{
			{ ResourceType.ClientVersion, "RSClientVersion" },
			{ ResourceType.Download, "RSDownload" },
			{ ResourceType.Patreon, "RSPatreon" },
			{ ResourceType.Github, "RSGithub" },
			{ ResourceType.Discord, "RSDiscord" },
			{ResourceType.FAQ, "RSFAQ"}
		};

		private static readonly Dictionary<ResourceType, string> ResCache = new Dictionary<ResourceType, string>();

		private const string BaseUrl = "https://api.ratscanner.com/v2";

		private const string GraphQLUrl = "https://tarkov-tools.com/graphql";

		private const string QuestInfoGraphQLQuery = "{\"query\":\"{quests {objectives {type targetItem {id} number}}}\"}";

		public static MarketItem[] GetMarketDB(Language language = Language.English)
		{
			try
			{
				using (var client = new WebClient())
				{
					var langString = LanguageMapping[language];
					var jsonGzipData = client.DownloadData($"{BaseUrl}/all?lang={langString}");
					var json = ExtractGZip(jsonGzipData);
					var marketItems = JsonConvert.DeserializeObject<MarketItem[]>(json);

					var questItems = ParseQuestItems(client.UploadString(GraphQLUrl, QuestInfoGraphQLQuery));
					foreach (MarketItem item in marketItems)
					{
						if (questItems.ContainsKey(item.Uid))
						{
							item.RequiredQuestCount = questItems[item.Uid];
						}
					}

					return marketItems;
				}
			}
			catch (Exception e)
			{
				Logger.LogError("Loading of DB failed.\n" + e);
				return null;
			}
		}

		public static string GetResource(ResourceType resource)
		{
			if (ResCache.ContainsKey(resource)) return ResCache[resource];

			if (!ResMapping.ContainsKey(resource))
			{
				Logger.LogError("Could not find resource mapping for: " + resource);
			}
			var resPath = ResMapping[resource];

			try
			{
				Logger.LogInfo("Loading resource \"" + resPath + "\"...");

				using (var client = new WebClient())
				{
					var jsonBytes = client.DownloadData(BaseUrl + "/res/" + resPath);
					var json = Encoding.UTF8.GetString(jsonBytes, 0, jsonBytes.Length);

					var value = JsonConvert.DeserializeObject<Resource>(json).Value;
					ResCache.Add(resource, value);
					return value;
				}
			}
			catch (Exception e)
			{
				Logger.LogError("Loading of resource \"" + resPath + "\" failed.", e);
				return "[Loading failed]";
			}
		}

		private static string ExtractGZip(byte[] gzip)
		{
			try
			{
				using (var outStream = new MemoryStream())
				using (var fSource = new MemoryStream(gzip))
				using (var csStream = new GZipStream(fSource, CompressionMode.Decompress))
				{
					var buffer = new byte[1024];
					int nRead;
					while ((nRead = csStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						outStream.Write(buffer, 0, nRead);
					}
					return Encoding.UTF8.GetString(outStream.ToArray());
				}
			}
			catch (Exception e)
			{
				Logger.LogError("Could not extract data.\n" + e);
				return "";
			}
		}

		private static Dictionary<string, int> ParseQuestItems(string json)
		{
			var questDict = new Dictionary<string, int>();
			try
			{
				var deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
				if (deserialized.ContainsKey("data"))
				{
					var dataDict = (deserialized["data"] as JObject).ToObject<Dictionary<string, object>>();
					if (dataDict.ContainsKey("quests"))
					{
						var questList = (dataDict["quests"] as JArray).ToObject<List<Dictionary<string, object>>>();
						foreach (Dictionary<string, object> quest in questList)
						{
							if (quest.ContainsKey("objectives"))
							{
								var objectives = (quest["objectives"] as JArray).ToObject<List<Dictionary<string, object>>>(); ;
								foreach (Dictionary<string, object> objective in objectives)
								{
									if (objective.ContainsKey("type") && objective.ContainsKey("number") && objective.ContainsKey("targetItem"))
									{
										if ((string)objective["type"] == "find")
										{
											var targetItemNumber = (int)(long)objective["number"];
											var targetItem = (objective["targetItem"] as JObject).ToObject<Dictionary<string, string>>();
											if (targetItem.ContainsKey("id"))
											{
												var targetItemId = targetItem["id"];

												var total = questDict.GetValueOrDefault(targetItemId, 0);
												total += targetItemNumber;
												questDict[targetItemId] = total;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.LogError("Failed to deserialize quest data", e);
			}

			return questDict;
		}
	}
}
