using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using RatScanner.FetchModels;

namespace RatScanner
{
	public static class ApiManager
	{
		public enum ResourceType
		{
			ClientVersion,
			Download,
			Patreon,
			Github,
			Discord,
		}

		private static readonly Dictionary<ResourceType, string> ResMapping = new Dictionary<ResourceType, string>
		{
			{ ResourceType.ClientVersion, "RSClientVersion" },
			{ ResourceType.Download, "RSDownload" },
			{ ResourceType.Patreon, "RSPatreon" },
			{ ResourceType.Github, "RSGithub" },
			{ ResourceType.Discord, "RSDiscord" },
		};

		private static readonly Dictionary<ResourceType, string> ResCache = new Dictionary<ResourceType, string>();

		private const string BaseUrl = "http://108.61.151.169:8080/api/v2";

		public static MarketItem[] GetMarketDB()
		{
			try
			{
				using (var client = new WebClient())
				{
					var jsonGzipData = client.DownloadData(BaseUrl + "/all");
					var json = ExtractGZip(jsonGzipData);

					return JsonConvert.DeserializeObject<MarketItem[]>(json);
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
	}
}
