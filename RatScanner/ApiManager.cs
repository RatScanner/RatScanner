using Newtonsoft.Json;
using RatScanner.FetchModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace RatScanner;

public static class ApiManager
{
	public enum ResourceType
	{
		ClientVersion,
		DownloadLink,
		PatreonLink,
		GithubLink,
		DiscordLink,
		FAQLink,
		ItemDataBundleLink,
		ItemDataBundleVersion,
		UpdaterLink,
	}

	private static readonly Dictionary<ResourceType, string> ResCache = new();

	// Official RatScanner API URL
	private const string BaseUrl = "https://api.ratscanner.com/v3";

	public static MarketItem[] GetMarketDB()
	{
		try
		{
			var json = GetString($"{BaseUrl}/all");
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

		var resPath = resource.GetResourcePath();

		try
		{
			Logger.LogInfo($"Loading resource \"{resPath}\"...");
			var json = GetString($"{BaseUrl}/res/{resPath}");
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
			var contents = GetBytes(url);
			File.WriteAllBytes(destination, contents);
		}
		catch (Exception e)
		{
			Logger.LogError($"Downloading of file \"{url}\" failed.", e);
		}
	}

	private static HttpWebRequest CreateRequest(string url, string bearerToken = null)
	{
		var request = WebRequest.CreateHttp(url);
		request.Method = WebRequestMethods.Http.Get;
		request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
		request.UserAgent = $"RatScanner-Client/{RatConfig.Version}";
		if (bearerToken != null) request.Headers.Add("Authorization", "Bearer " + bearerToken);
		return request;
	}

	private static byte[] GetBytes(string url, string bearerToken = null)
	{
		using var response = (HttpWebResponse)CreateRequest(url, bearerToken).GetResponse();
		using var stream = response.GetResponseStream();
		using var memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private static string GetString(string url, string bearerToken = null)
	{
		using var response = (HttpWebResponse)CreateRequest(url, bearerToken).GetResponse();
		using var stream = response.GetResponseStream();
		var noEncoding = string.IsNullOrEmpty(response.CharacterSet);
		var encoding = noEncoding ? Encoding.UTF8 : Encoding.GetEncoding(response.CharacterSet);
		var reader = new StreamReader(stream, encoding);
		return reader.ReadToEnd();
	}

	public static string GetResourcePath(this ResourceType resourceType)
	{
		return resourceType switch
		{
			ResourceType.ClientVersion => "RSClientVersion",
			ResourceType.DownloadLink => "RSDownloadLink",
			ResourceType.PatreonLink => "RSPatreonLink",
			ResourceType.GithubLink => "RSGithubLink",
			ResourceType.DiscordLink => "RSDiscordLink",
			ResourceType.FAQLink => "RSFAQLink",
			ResourceType.ItemDataBundleLink => "RSItemDataBundleLink",
			ResourceType.ItemDataBundleVersion => "RSItemDataBundleVersion",
			ResourceType.UpdaterLink => "RSUpdaterLink",
			_ => throw new NotImplementedException(),
		};
	}
}
