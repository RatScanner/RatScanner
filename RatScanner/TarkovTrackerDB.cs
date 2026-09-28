using Newtonsoft.Json;
using RatScanner.FetchModels.TarkovTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RatScanner;

// Storing information about progression from TarkovTracker API
public class TarkovTrackerDB {
	private TokenResponse? _token;
	private bool _badToken;

	// Last token validation result, kept across page instances so revisiting the
	// settings does not re-issue a network request for a token we already checked.
	// Both outcomes are cached; only a changed token or backend invalidates it.
	private static string _lastValidatedToken = "";
	private static RatConfig.TarkovTrackerBackend _lastValidatedBackend;
	private static bool _lastValidationResult;

	public List<UserProgress> Progress = [];
	public string Self = "";
	public string? Token;

	/// <summary>
	/// Returns the cached validation result for the given token and backend,
	/// or null when that pair has not been checked yet.
	/// </summary>
	internal static bool? GetCachedValidation(string token, RatConfig.TarkovTrackerBackend backend) {
		if (string.IsNullOrEmpty(token)) {
			return false;
		} else if (token != _lastValidatedToken || backend != _lastValidatedBackend) {
			return null;
		} else {
			return _lastValidationResult;
		}
	}

	/// <summary>
	/// Records the outcome of a token validation for later reuse.
	/// </summary>
	internal static void CacheValidation(string token, RatConfig.TarkovTrackerBackend backend, bool result) {
		_lastValidatedToken = token;
		_lastValidatedBackend = backend;
		_lastValidationResult = result;
	}

	/// <summary>
	/// Discards the cached validation, forcing the next check to hit the network.
	/// </summary>
	internal static void InvalidateCachedValidation() {
		_lastValidatedToken = "";
		_lastValidationResult = false;
	}

	// Set up the TarkovTracker DB
	public bool Init() {
		if (!ValidToken()) return false;
		UpdateProgression();
		return true;
	}

	private bool ValidToken() {
		if (Token != null && Token.Length > 0) {
			// We have a Token in our config, check it
			if (_token != null) {
				if (_token.Id != Token)
					// Token in config versus last attempted token are different
					UpdateToken();
			} else {
				// We have a token config, but haven't tried validating the token
				UpdateToken();
			}

			return !_badToken;
		}

		return false;
	}

	public void UpdateToken() {
		// Attempt to verify the token
		try {
			var newToken = GetToken();
			_badToken = newToken == null;
			if (!_badToken)
				// We have a valid token
				_token = newToken;
		} catch (RateLimitExceededException) {
			// We hit a rate limit issue, this doesn't mean our token is bad, but we have to wait until we try again
		} catch (UnauthorizedTokenException) {
			// We have an unauthorized token, retrying won't help until we change it
			_badToken = true;
			_token = new TokenResponse { Id = Token };
		}
	}

	public int TeammateCount => Progress.Count(x => x.UserId != Self);

	public bool? TeamProgressAvailable => _token?.Permissions.Contains("TP");

	public bool? SoloProgressAvailable => _token?.Permissions.Contains("GP");

	private void UpdateProgression() {
		try {
			if (TeamProgressAvailable == true) {

				var tpr = GetTeamProgress();
				Self = tpr.Meta.Self;
				Progress = [.. tpr.TeamProgress.Where(x => !tpr.Meta.HiddenTeammates.Contains(x.UserId))];
			} else if (SoloProgressAvailable == true) {
				var spr = GetProgress();
				Self = spr.Meta.Self;
				Progress = [spr.UserProgress];
			} else {
				// We dont have permissions
			}
		} catch (RateLimitExceededException) {
			// We hit a rate limit issue, this doesn't mean our token is bad, but we have to wait until we try again
		} catch (UnauthorizedTokenException) {
			// We have an unauthorized token exception, it could be that we don't have permissions for this call
		} catch (JsonReaderException) {
			// We do not want to crash an entire application just because of invalid 3rd party api response
		}
	}

	// Checks the token metadata endpoint for TarkovTracker
	private TeamProgressResponse GetTeamProgress() {
		try {
			var responseStr = APIClient.Get($"{RatConfig.Tracking.TarkovTracker.Endpoint}/team/progress", _token.Id);
			return JsonConvert.DeserializeObject<TeamProgressResponse>(responseStr) ?? new();
		} catch (WebException e) {
			var status = (e.Response as HttpWebResponse)?.StatusCode;
			if (status is HttpStatusCode.TooManyRequests)
				throw new RateLimitExceededException("Rate Limiting reached for token", e);
			// Unknown error, continue throwing
			throw;
		}
	}

	// Checks the token metadata endpoint for TarkovTracker
	private ProgressResponse GetProgress() {
		try {
			var responseStr = APIClient.Get($"{RatConfig.Tracking.TarkovTracker.Endpoint}/progress", _token.Id);
			return JsonConvert.DeserializeObject<ProgressResponse>(responseStr) ?? new();
		} catch (WebException e) {
			var status = (e.Response as HttpWebResponse)?.StatusCode;
			if (status is HttpStatusCode.TooManyRequests)
				throw new RateLimitExceededException("Rate Limiting reached for token", e);
			// Unknown error, continue throwing
			throw;
		}
	}

	public bool TestToken(string test_token) {
		try {
			_ = GetToken(test_token);
		} catch (Exception) {
			return false;
		}

		return true;
	}

	// Checks the token metadata endpoint for TarkovTracker
	private TokenResponse GetToken(string? custom_token = null) {
		var working_token = Token;
		if (custom_token != null) working_token = custom_token;

		var responseStr = APIClient.Get($"{RatConfig.Tracking.TarkovTracker.Endpoint}/token", working_token);
		var result = JsonConvert.DeserializeObject<TokenResponse>(responseStr);
		return result ?? throw new Exception("Failed to deserialize token response");
	}
}
