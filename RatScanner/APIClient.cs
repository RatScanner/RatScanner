using RatScanner.FetchModels.TarkovTracker;
using System.Net;
using System.Net.Http;

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
		HttpRequestMessage request = formRequest(HttpMethod.Get, url, bearerToken);
		System.Threading.Tasks.Task<HttpResponseMessage> responseTask = httpClient.SendAsync(request);
		responseTask.Wait();
		if (responseTask.Result.StatusCode == HttpStatusCode.Unauthorized)
			throw new UnauthorizedTokenException("Token was rejected by the API");
		else if (responseTask.Result.StatusCode == HttpStatusCode.TooManyRequests)
			throw new RateLimitExceededException("Rate Limiting reached for token");
		System.Threading.Tasks.Task<string> contentTask = responseTask.Result.Content.ReadAsStringAsync();
		contentTask.Wait();

		return contentTask.Result;
	}
}
