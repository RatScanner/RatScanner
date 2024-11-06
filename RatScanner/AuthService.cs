using System.Net.Http;
using System.Threading.Tasks;

namespace RatScanner;
internal class AuthService {
	static readonly HttpClient HttpClient = new();

	public static async void AuthDiscord() {
		OAuth2.Client client = new() {
			DisplayName = "Discord",
			Id = "1146972103736819736",
			AuthorizationEndpoint = "https://discord.com/oauth2/authorize",
			TokenEndpoint = "https://discord.com/api/oauth2/token",
			RedirectUri = "http://127.0.0.1:42252/",
			Scope = "connections+guilds+guilds.members.read",
		};

		OAuth2.Token? token = new() {
			AccessToken = "",
			RefreshToken = RatConfig.OAuthRefreshToken.Discord,
		};

		if (!string.IsNullOrEmpty(RatConfig.OAuthRefreshToken.Discord)) {
			token = await ApiManager.ExchangeRefreshTokenForTokensAsync(client, token);
			token ??= await OAuth2.DoOAuthAsync(client);
		} else token = await OAuth2.DoOAuthAsync(client);

		if (token == null) {
			Logger.LogWarning("Discord auth failed.");
			return;
		}

		RatConfig.OAuthRefreshToken.Discord = token.RefreshToken;
		RatConfig.SaveConfig();

		await Request(token, "https://discord.com/api/users/@me/guilds/687549250435153930/member");
	}

	private static async Task Request(OAuth2.Token token, string uri) {
		HttpRequestMessage request = new() {
			Method = HttpMethod.Get,
			RequestUri = new System.Uri(uri),
			Headers = {
				{ "Authorization", $"Bearer {token.AccessToken}" },
			},
		};
		HttpResponseMessage response = await HttpClient.SendAsync(request);

		Logger.LogInfo($"STATUS CODE: {response.StatusCode}");
		string content = await response.Content.ReadAsStringAsync();
		Logger.LogInfo(content);
	}

}

//await RequestUserInfoAsync(accessToken, "https://discord.com/api/users/@me/guilds");
//await RequestUserInfoAsync(tokenPair.AccessToken, "https://discord.com/api/users/@me/guilds/687549250435153930/member");
