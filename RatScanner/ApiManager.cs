using Newtonsoft.Json;
using RatScanner.FetchModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace RatScanner;

public static class ApiManager {
	public enum ResourceType {
		ClientVersion,
		ClientForceUpdateVersions,
		DownloadLink,
		PatreonLink,
		GithubLink,
		DiscordLink,
		FAQLink,
		UpdaterLink,
	}

	private static readonly Dictionary<ResourceType, string> ResCache = new();

	// Official RatScanner API URL
	private const string BaseUrl = "https://api.ratscanner.com/v3";

	public static string GetResource(ResourceType resource) {
		if (ResCache.ContainsKey(resource)) return ResCache[resource];

		string resPath = resource.GetResourcePath();

		try {
			Logger.LogInfo($"Loading resource \"{resPath}\"...");
			string json = GetString($"{BaseUrl}/res/{resPath}");
			string value = JsonConvert.DeserializeObject<Resource>(json)?.Value ?? throw new NullReferenceException();
			ResCache.Add(resource, value);
			return value;
		} catch (Exception e) {
			Logger.LogError($"Loading of resource \"{resPath}\" failed.", e);
			return "[Loading failed]";
		}
	}

	public static void DownloadFile(string url, string destination) {
		try {
			Logger.LogInfo($"Downloading file \"{url}\"...");
			byte[] contents = GetBytes(url);
			File.WriteAllBytes(destination, contents);
		} catch (Exception e) {
			Logger.LogError($"Downloading of file \"{url}\" failed.", e);
		}
	}

	private static HttpWebRequest CreateRequest(string url, string? bearerToken = null) {
		HttpWebRequest request = WebRequest.CreateHttp(url);
		request.Method = WebRequestMethods.Http.Get;
		request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
		request.UserAgent = $"RatScanner-Client/{RatConfig.Version}";
		if (bearerToken != null) request.Headers.Add("Authorization", "Bearer " + bearerToken);
		return request;
	}

	private static byte[] GetBytes(string url, string? bearerToken = null) {
		using HttpWebResponse response = (HttpWebResponse)CreateRequest(url, bearerToken).GetResponse();
		using Stream stream = response.GetResponseStream();
		using MemoryStream memoryStream = new();
		stream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private static string GetString(string url, string? bearerToken = null) {
		using HttpWebResponse response = (HttpWebResponse)CreateRequest(url, bearerToken).GetResponse();
		using Stream stream = response.GetResponseStream();
		bool noEncoding = string.IsNullOrEmpty(response.CharacterSet);
		Encoding encoding = noEncoding ? Encoding.UTF8 : Encoding.GetEncoding(response.CharacterSet);
		StreamReader reader = new(stream, encoding);
		return reader.ReadToEnd();
	}

	public static string GetResourcePath(this ResourceType resourceType) {
		return resourceType switch {
			ResourceType.ClientVersion => "RSClientVersion",
			ResourceType.ClientForceUpdateVersions => "RSClientForceUpdateVersions",
			ResourceType.DownloadLink => "RSDownloadLink",
			ResourceType.PatreonLink => "RSPatreonLink",
			ResourceType.GithubLink => "RSGithubLink",
			ResourceType.DiscordLink => "RSDiscordLink",
			ResourceType.FAQLink => "RSFAQLink",
			ResourceType.UpdaterLink => "RSUpdaterLink",
			_ => throw new NotImplementedException(),
		};
	}
}
