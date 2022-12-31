using System.Net;
using System.Net.Http.Json;

namespace RatTracking;

/// <todo>
/// We absolutely want to move this into the main project at some point
/// </todo>
public class TarkovDevAPI
{
	const string ApiEndpoit = "https://api.tarkov.dev/graphql";

	private static readonly HttpClient httpClient = new(new HttpClientHandler
	{
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
	});

	private static HttpRequestMessage Request(HttpMethod method, string url)
	{
		var request = new HttpRequestMessage(method, url);
		request.Headers.Add("User-Agent", "RatScanner-Client/3");

		return request;
	}

	private static string Get(string query)
	{
		var body = new Dictionary<string, string>() { { "query", query } };
		var responseTask = httpClient.PostAsJsonAsync(ApiEndpoit, body);
		responseTask.Wait();
		if (responseTask.Result.StatusCode != HttpStatusCode.OK) throw new Exception("Tarkov.dev API request failed.");
		var contentTask = responseTask.Result.Content.ReadAsStringAsync();
		contentTask.Wait();

		return contentTask.Result;
	}

	/// <summary>
	/// Querys all tasks
	/// </summary>
	/// <returns></returns>
	// TODO: create task reponse class similar to the response classes
	// of tarkov-tracker api in the fetch models namespace. The json
	// response from the api should be able to just be casted into
	// that class using jsonconvert.deserialize
	//	public static TaskResponse GetTasks()
	//	{
	//		// We REALLY want to cache this for probably the whole execution time of rat scanner
	//		// adding caching at a higher level would proba
	//		return JsonConvert.Deserialize<TaskResponse>(Get("{tasks(limit: 0)}"));
	//	}
}
