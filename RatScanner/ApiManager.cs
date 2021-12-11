﻿using Newtonsoft.Json;
using RatScanner.FetchModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

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
			{Language.English, "en"},
			{Language.Russian, "ru"},
			{Language.German, "de"},
			{Language.French, "fr"},
			{Language.Spanish, "es"},
			{Language.Chinese, "cn"},
		};

		public enum ResourceType
		{
			ClientVersion,
			DownloadLink,
			PatreonLink,
			GithubLink,
			DiscordLink,
			FAQLink,
			ItemDataLink,
			ItemDataVersion,
		}

		private static readonly Dictionary<ResourceType, string> ResMapping = new Dictionary<ResourceType, string>
		{
			{ResourceType.ClientVersion, "RSClientVersion"},
			{ResourceType.DownloadLink, "RSDownloadLink"},
			{ResourceType.PatreonLink, "RSPatreonLink"},
			{ResourceType.GithubLink, "RSGithubLink"},
			{ResourceType.DiscordLink, "RSDiscordLink"},
			{ResourceType.FAQLink, "RSFAQLink"},
			{ResourceType.ItemDataLink, "RSItemDataLink"},
			{ResourceType.ItemDataVersion, "RSItemDataVersion"},
		};

		private static readonly Dictionary<ResourceType, string> ResCache = new Dictionary<ResourceType, string>();

		// Official RatScanner API URL
		private const string BaseUrl = "https://api.ratscanner.com/v3";

		public static MarketItem[] GetMarketDB(Language language = Language.English)
		{
			try
			{
				var langString = LanguageMapping[language];
				var json = Get($"{BaseUrl}/all?lang={langString}");
				return JsonConvert.DeserializeObject<MarketItem[]>(json);
			}
			catch (Exception e)
			{
				Logger.LogError($"Loading of market data failed.\n{e}");
				return null;
			}
		}

		public static string GetResource(ResourceType resource)
		{
			if (ResCache.ContainsKey(resource)) return ResCache[resource];

			if (!ResMapping.ContainsKey(resource)) Logger.LogError($"Could not find resource mapping for: {resource}");
			var resPath = ResMapping[resource];

			try
			{
				Logger.LogInfo($"Loading resource \"{resPath}\"...");
				var json = Get($"{BaseUrl}/res/{resPath}");
				var value = JsonConvert.DeserializeObject<Resource>(json)?.Value;
				ResCache.Add(resource, value);
				return value;
			}
			catch (Exception e)
			{
				Logger.LogError($"Loading of resource \"{resPath}\" failed.", e);
				return "[Loading failed]";
			}
		}

		public static void DownloadFile(string url, string destination)
		{
			try
			{
				Logger.LogInfo($"Downloading file \"{url}\"...");
				var contents = Get(url);
				File.WriteAllText(destination, contents);
			}
			catch (Exception e)
			{
				Logger.LogError($"Downloading of file \"{url}\" failed.", e);
			}
		}

		private static string Get(string url, string bearerToken = null)
		{
			var request = WebRequest.CreateHttp(url);
			request.Method = WebRequestMethods.Http.Get;
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			request.UserAgent = $"RatScanner-Client/{RatConfig.Version}";
			if (bearerToken != null) request.Headers.Add("Authorization", "Bearer " + bearerToken);

			using var response = (HttpWebResponse)request.GetResponse();
			using var stream = response.GetResponseStream();

			var noEncoding = string.IsNullOrEmpty(response.CharacterSet);
			var encoding = noEncoding ? Encoding.UTF8 : Encoding.GetEncoding(response.CharacterSet);
			var reader = new StreamReader(stream, encoding);
			return reader.ReadToEnd();
		}
	}
}
