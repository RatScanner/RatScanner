using RatScanner.FetchModels.TarkovTracker;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace RatScanner;

internal static class APIClient {
	private static readonly HttpClient httpClient = new(new HttpClientHandler {
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
	});

	private static HttpRequestMessage formRequest(HttpMethod method, string url, string? bearerToken = null) {
		HttpRequestMessage request = new(method, url);
		request.Headers.Add("User-Agent", "RatScanner-Client/3");
		if (bearerToken != null) request.Headers.Add("Authorization", "Bearer " + bearerToken);

		return request;
	}

	public static string Get(string url, string? bearerToken = null) {
		var request = formRequest(HttpMethod.Get, url, bearerToken);
		var responseTask = httpClient.SendAsync(request);
		responseTask.Wait();
		if (responseTask.Result.StatusCode == HttpStatusCode.Unauthorized)
			throw new UnauthorizedTokenException("Token was rejected by the API");
		else if (responseTask.Result.StatusCode == HttpStatusCode.TooManyRequests)
			throw new RateLimitExceededException("Rate Limiting reached for token");
		var contentTask = responseTask.Result.Content.ReadAsStringAsync();
		contentTask.Wait();

		return contentTask.Result;
	}

	public static string Post(string url, object body, string bearerToken) {
		using var request = formRequest(HttpMethod.Post, url, bearerToken);
		request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

		using var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
		if (response.StatusCode == HttpStatusCode.Unauthorized)
			throw new UnauthorizedTokenException("Token was rejected by the API");
		if (response.StatusCode == HttpStatusCode.Forbidden)
			throw new TrackerApiException(response.StatusCode, "Tracker token lacks the required write permission");
		if (response.StatusCode == HttpStatusCode.TooManyRequests)
			throw new RateLimitExceededException("Rate limiting reached for token");

		var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
		if (!response.IsSuccessStatusCode)
			throw new TrackerApiException(response.StatusCode, $"Tracker API returned {(int)response.StatusCode}: {content}");

		return content;
	}
}
