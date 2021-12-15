using System.Net;

namespace RatTracking
{
	static class APIClient
	{
		static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
		{
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
		});

		private static HttpRequestMessage formRequest(HttpMethod method, string url, string bearerToken = null)
		{
			HttpRequestMessage request = new HttpRequestMessage(method, url);
			request.Headers.Add("User-Agent", "RatScanner-Client/3");
			if (bearerToken != null) request.Headers.Add("Authorization", "Bearer " + bearerToken);

			return request;
		}

		public static string Get(string url, string bearerToken = null)
		{
			HttpRequestMessage request = formRequest(HttpMethod.Get, url, bearerToken);
			Task<HttpResponseMessage> responseTask = httpClient.SendAsync(request);
			responseTask.Wait();
			Task<string> contentTask = responseTask.Result.Content.ReadAsStringAsync();
			contentTask.Wait();

			return contentTask.Result;
		}
	}
}
