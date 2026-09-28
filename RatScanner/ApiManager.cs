using Newtonsoft.Json;
using RatScanner.FetchModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static RatScanner.OAuth2;

namespace RatScanner;

public static class ApiManager {
	private static readonly HttpClient HttpClient = new();

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

	private static readonly Dictionary<ResourceType, string> ResCache = [];

	// Official RatScanner API URL
	private const string BaseUrl = "https://api.ratscanner.com/v3";

	internal static async Task<Token?> ExchangeRefreshTokenForTokensAsync(Client client, Token token) {
		Logger.LogInfo("Exchanging refresh token for tokens...");

		var content = JsonContent.Create(new { client_id = client.Id, refresh_token = token.RefreshToken });
		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{BaseUrl}/oauth/refresh"),
			Content = content,
		};

		var response = await HttpClient.SendAsync(request);
		var responseText = await response.Content.ReadAsStringAsync();

		if (!response.IsSuccessStatusCode) {
			Logger.LogWarning($"STATUS CODE: {response.StatusCode}");
			Logger.LogInfo($"Content: {responseText}");
			return null;
		}

		var tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

		return new Token() {
			AccessToken = tokenEndpointDecoded["access_token"],
			RefreshToken = tokenEndpointDecoded["refresh_token"],
		};
	}

	public static string GetResource(ResourceType resource) {
		if (ResCache.TryGetValue(resource, out var value)) return value;

		var resPath = resource.GetResourcePath();

		try {
			Logger.LogInfo($"Loading resource \"{resPath}\"...");
			var json = GetString($"{BaseUrl}/res/{resPath}");
			var resourceValue = JsonConvert.DeserializeObject<Resource>(json)?.Value ?? throw new NullReferenceException();
			ResCache.Add(resource, resourceValue);
			return resourceValue;
		} catch (Exception e) {
			Logger.LogError($"Loading of resource \"{resPath}\" failed.", e);
			return "[Loading failed]";
		}
	}

	public static void DownloadFile(string url, string destination) {
		try {
			Logger.LogInfo($"Downloading file \"{url}\"...");
			var contents = GetBytes(url);
			File.WriteAllBytes(destination, contents);
		} catch (Exception e) {
			Logger.LogError($"Downloading of file \"{url}\" failed.", e);
		}
	}

	private static HttpWebRequest CreateRequest(string url, string? bearerToken = null) {
		var request = WebRequest.CreateHttp(url);
		request.Method = WebRequestMethods.Http.Get;
		request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
		request.UserAgent = $"RatScanner-Client/{RatConfig.Version}";
		if (bearerToken != null) request.Headers.Add("Authorization", "Bearer " + bearerToken);
		return request;
	}

	private static byte[] GetBytes(string url, string? bearerToken = null) {
		using var response = (HttpWebResponse)CreateRequest(url, bearerToken).GetResponse();
		using var stream = response.GetResponseStream();
		using MemoryStream memoryStream = new();
		stream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private static string GetString(string url, string? bearerToken = null) {
		using var response = (HttpWebResponse)CreateRequest(url, bearerToken).GetResponse();
		using var stream = response.GetResponseStream();
		var noEncoding = string.IsNullOrEmpty(response.CharacterSet);
		var encoding = noEncoding ? Encoding.UTF8 : Encoding.GetEncoding(response.CharacterSet);
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
