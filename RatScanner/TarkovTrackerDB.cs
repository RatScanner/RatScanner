using Newtonsoft.Json;
using RatScanner.FetchModels.TarkovTracker;
using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RatScanner;

// Storing information about progression from TarkovTracker API
public class TarkovTrackerDB {
	private TokenResponse? _token;
	private bool _badToken;
	private GameMode _mode;

	// Last token validation result, kept across page instances so revisiting the
	// settings does not re-issue a network request for a token we already checked.
	// Both outcomes are cached; only a changed token or backend invalidates it.
	private static string _lastValidatedToken = "";
	private static bool _lastValidationResult;

	public List<UserProgress> Progress = [];
	public string Self = "";
	public string? Token;

	internal event Action? Changed;

	/// <summary>
	/// Returns the cached validation result for the given token and backend,
	/// or null when that pair has not been checked yet.
	/// </summary>
	internal static bool? GetCachedValidation(string token) {
		if (string.IsNullOrEmpty(token)) {
			return false;
		} else if (token != _lastValidatedToken) {
			return null;
		} else {
			return _lastValidationResult;
		}
	}

	/// <summary>
	/// Records the outcome of a token validation for later reuse.
	/// </summary>
	internal static void CacheValidation(string token, bool result) {
		_lastValidatedToken = token;
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
		SetGameMode(RatConfig.GameMode);
		if (!ValidToken()) return false;
		UpdateProgression();
		Changed?.Invoke();
		return true;
	}

	internal void SetGameMode(GameMode mode) {
		if (_mode == mode && Token == RatConfig.Tracking.TarkovTracker.TokenFor(mode)) return;

		_mode = mode;
		Token = RatConfig.Tracking.TarkovTracker.TokenFor(mode);
		_token = null;
		_badToken = false;
		Progress = [];
		Self = "";
	}

	internal void SetTaskState(string taskId, string state) {
		if (string.IsNullOrWhiteSpace(taskId)) return;
		EnsureWritableToken();

		_ = APIClient.Post(
			$"{RatConfig.Tracking.TarkovTracker.Endpoint}/progress/task/{Uri.EscapeDataString(taskId)}",
			new { state },
			Token!);
		RefreshAfterWrite();
	}

	internal void SetObjectiveState(string objectiveId, bool complete, int? count = null) {
		if (string.IsNullOrWhiteSpace(objectiveId)) return;
		EnsureWritableToken();

		var state = complete ? "completed" : "uncompleted";
		object body = count.HasValue ? new { state, count } : new { state };
		_ = APIClient.Post(
			$"{RatConfig.Tracking.TarkovTracker.Endpoint}/progress/task/objective/{Uri.EscapeDataString(objectiveId)}",
			body,
			Token!);
		RefreshAfterWrite();
	}

	internal void SetObjectiveCount(string objectiveId, int count) {
		if (string.IsNullOrWhiteSpace(objectiveId)) return;
		EnsureWritableToken();

		_ = APIClient.Post(
			$"{RatConfig.Tracking.TarkovTracker.Endpoint}/progress/task/objective/{Uri.EscapeDataString(objectiveId)}",
			new { count },
			Token!);
		RefreshAfterWrite();
	}

	private void EnsureWritableToken() {
		SetGameMode(RatConfig.GameMode);
		if (string.IsNullOrWhiteSpace(Token))
			throw new UnauthorizedTokenException($"No TarkovTracker token is configured for {RatConfig.GameMode}");
		if (_token?.Permissions.Contains("WP") != true) UpdateToken();
		if (_token?.Permissions.Contains("WP") != true)
			throw new TrackerApiException(HttpStatusCode.Forbidden, "TarkovTracker token lacks write permission");
	}

	internal void RaiseChanged() => Changed?.Invoke();

	private void RefreshAfterWrite() {
		// A write-only token cannot read progress back, so the refresh only runs when
		// the token can read it. The change event always fires so the views repaint.
		if (SoloProgressAvailable == true || TeamProgressAvailable == true) UpdateProgression();
		Changed?.Invoke();
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
