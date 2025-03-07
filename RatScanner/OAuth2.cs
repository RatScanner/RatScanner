using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner;
internal static class OAuth2 {
	static readonly HttpClient HttpClient = new();

	internal class Client {
		internal required string DisplayName;
		internal required string Id;
		internal required string AuthorizationEndpoint;
		internal required string TokenEndpoint;
		internal required string RedirectUri;
		internal required string Scope;
	}

	internal class Token {
		internal required string AccessToken;
		internal required string RefreshToken;
	}

	internal static async Task<Token?> DoOAuthAsync(Client client) {
		// Generates state and PKCE values
		string state = GenerateRandomDataBase64url(32);
		string codeVerifier = GenerateRandomDataBase64url(32);
		string codeChallenge = Base64UrlEncodeNoPadding(Sha256Ascii(codeVerifier));

		// Creates an HttpListener to listen for requests on the redirect URI
		HttpListener http = new();
		http.Prefixes.Add(client.RedirectUri);
		Logger.LogInfo("Listening..");
		http.Start();

		// Creates the OAuth 2.0 authorization request
		string authorizationRequest = string.Format("{0}?response_type=code&scope={1}&redirect_uri={2}&client_id={3}&state={4}&code_challenge={5}&code_challenge_method=S256",
			client.AuthorizationEndpoint,
			client.Scope,
			client.RedirectUri,
			client.Id,
			state,
			codeChallenge);

		// Opens request in the browser
		ProcessStartInfo psi = new() {
			FileName = authorizationRequest,
			UseShellExecute = true,
		};
		Process.Start(psi);

		// Waits for the OAuth authorization response
		HttpListenerContext context = await http.GetContextAsync();

		// Sends an HTTP response to the browser
		HttpListenerResponse response = context.Response;
		string responseString = "<html><body><h1>You can close this window now.</h1></body></html>";
		byte[] buffer = Encoding.UTF8.GetBytes(responseString);
		response.ContentLength64 = buffer.Length;
		Stream responseOutput = response.OutputStream;
		await responseOutput.WriteAsync(buffer, 0, buffer.Length);
		responseOutput.Close();
		http.Stop();
		Logger.LogInfo("HTTP server stopped.");

		// Checks for errors
		string? error = context.Request.QueryString.Get("error");
		if (error is not null) {
			Logger.LogInfo($"OAuth authorization error: {error}.");
			return null;
		}
		if (context.Request.QueryString.Get("code") is null
			|| context.Request.QueryString.Get("state") is null) {
			Logger.LogInfo($"Malformed authorization response. {context.Request.QueryString}");
			return null;
		}

		// extracts the code
		string? code = context.Request.QueryString.Get("code");
		string? incomingState = context.Request.QueryString.Get("state");

		// Compares the receieved state to the expected value, to ensure that
		// this app made the request which resulted in authorization
		if (incomingState != state) {
			Logger.LogInfo($"Received request with invalid state ({incomingState})");
			return null;
		}
		Logger.LogInfo("Authorization code: " + code);

		// Starts the code exchange at the Token Endpoint
		return await ExchangeCodeForTokensAsync(client, code, codeVerifier);
	}

	static async Task<Token?> ExchangeCodeForTokensAsync(Client client, string code, string codeVerifier) {
		Logger.LogInfo("Exchanging code for tokens...");

		FormUrlEncodedContent content = new([
			new("grant_type", "authorization_code"),
			new("code", code),
			new("redirect_uri", client.RedirectUri),
			new("client_id", client.Id),
			new("code_verifier", codeVerifier)
		]);
		return await RequestTokensAsync(client, content);
	}

	internal static async Task<Token?> ExchangeRefreshTokenForTokensAsync(Client client, Token token) {
		Logger.LogInfo("Exchanging refresh token for tokens...");

		FormUrlEncodedContent content = new([
			new("grant_type", "refresh_token"),
			new("refresh_token", token.RefreshToken),
			new("client_id", client.Id),
		]);
		return await RequestTokensAsync(client, content);
	}

	static async Task<Token?> RequestTokensAsync(Client client, FormUrlEncodedContent content) {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Post,
			RequestUri = new Uri(client.TokenEndpoint),
			Headers = {
				{ "ContentType", $"application/x-www-form-urlencoded" },
			},
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

	/// <summary>
	/// Returns URI-safe data with a given input length.
	/// </summary>
	/// <param name="length">Input length (nb. output will be longer)</param>
	private static string GenerateRandomDataBase64url(int length) {
		byte[] bytes = RandomNumberGenerator.GetBytes(length);
		return Base64UrlEncodeNoPadding(bytes);
	}

	/// <summary>
	/// Returns the SHA256 hash of the input string, which is assumed to be ASCII.
	/// </summary>
	private static byte[] Sha256Ascii(string text) {
		byte[] bytes = Encoding.ASCII.GetBytes(text);
		return SHA256.HashData(bytes);
	}

	/// <summary>
	/// Base64url no-padding encodes the given input buffer.
	/// </summary>
	private static string Base64UrlEncodeNoPadding(byte[] buffer) {
		string base64 = Convert.ToBase64String(buffer);

		// Converts base64 to base64url.
		base64 = base64.Replace("+", "-");
		base64 = base64.Replace("/", "_");
		// Strips padding.
		base64 = base64.Replace("=", "");

		return base64;
	}
}
