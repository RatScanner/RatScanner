using System.Net;
using RatTracking.FetchModels.TarkovTracker;

namespace RatTracking;

internal static class APIClient
{
	private static readonly HttpClient httpClient = new(new HttpClientHandler
	{
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
	});

	private static HttpRequestMessage formRequest(HttpMethod method, string url, string bearerToken = null)
	{
		var request = new HttpRequestMessage(method, url);
		request.Headers.Add("User-Agent", "RatScanner-Client/3");
		if (bearerToken != null) request.Headers.Add("Authorization", "Bearer " + bearerToken);

		return request;
	}

	public static string Get(string url, string bearerToken = null)
	{
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
}
