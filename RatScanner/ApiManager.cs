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
	static readonly HttpClient HttpClient = new();

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

	internal static async Task<OAuth2.Token?> ExchangeRefreshTokenForTokensAsync(Client client, Token token) {
		Logger.LogInfo("Exchanging refresh token for tokens...");

		JsonContent content = JsonContent.Create(new { client_id = client.Id, refresh_token = token.RefreshToken });
		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri($"{BaseUrl}/oauth/refresh"),
			Content = content,
		};

		HttpResponseMessage response = await HttpClient.SendAsync(request);
		string responseText = await response.Content.ReadAsStringAsync();

		if (!response.IsSuccessStatusCode) {
			Logger.LogWarning($"STATUS CODE: {response.StatusCode}");
			Logger.LogInfo($"Content: {responseText}");
			return null;
		}

		Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

		return new Token() {
			AccessToken = tokenEndpointDecoded["access_token"],
			RefreshToken = tokenEndpointDecoded["refresh_token"],
		};
	}

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
