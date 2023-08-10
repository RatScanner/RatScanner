﻿using Newtonsoft.Json;
using RatScanner.FetchModels.TarkovTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RatScanner;

// Storing information about progression from TarkovTracker API
public class TarkovTrackerDB
{
	private TokenResponse? _token;
	private bool _badToken;

	public List<UserProgress> Progress = new();
	public string Self = "";
	public string? Token;

	private const string TarkovTrackerUrl = "https://tarkovtracker.io/api/v2";

	// Set up the TarkovTracker DB
	public bool Init()
	{
		if (!ValidToken()) return false;
		UpdateProgression();
		return true;
	}

	private bool ValidToken()
	{
		if (Token != null && Token.Length > 0)
		{
			// We have a Token in our config, check it
			if (_token != null)
			{
				if (_token.Id != Token)
					// Token in config versus last attempted token are different
					UpdateToken();
			}
			else
			{
				// We have a token config, but haven't tried validating the token
				UpdateToken();
			}

			return !_badToken;
		}

		return false;
	}

	private void UpdateToken()
	{
		// Attempt to verify the token
		try
		{
			var newToken = GetToken();
			_badToken = newToken == null;
			if (!_badToken)
				// We have a valid token
				_token = newToken;
		}
		catch (RateLimitExceededException)
		{
			// We hit a rate limit issue, this doesn't mean our token is bad, but we have to wait until we try again
		}
		catch (UnauthorizedTokenException)
		{
			// We have an unauthorized token, retrying won't help until we change it
			_badToken = true;
			_token = new TokenResponse { Id = Token };
		}
	}

	public int TeammateCount => Progress.Count(x => x.UserId != Self);

	public bool? TeamProgressAvailable => _token?.Permissions.Contains("TP");

	public bool? SoloProgressAvailable => _token?.Permissions.Contains("GP");

	private void UpdateProgression()
	{
		try
		{
			if (TeamProgressAvailable == true)
			{

				var tpr = GetTeamProgress();
				Self = tpr.Meta.Self;
				Progress = tpr.TeamProgress.Where(x => !tpr.Meta.HiddenTeammates.Contains(x.UserId)).ToList();
			}
			else if (SoloProgressAvailable == true)
			{
				var spr = GetProgress();
				Self = spr.Meta.Self;
				Progress = new List<UserProgress> { spr.UserProgress };
			}
			else
			{
				// We dont have permissions
			}
		}
		catch (RateLimitExceededException)
		{
			// We hit a rate limit issue, this doesn't mean our token is bad, but we have to wait until we try again
		}
		catch (UnauthorizedTokenException)
		{
			// We have an unauthorized token exception, it could be that we don't have permissions for this call
		}
		catch (JsonReaderException)
		{
			// We do not want to crash an entire application just because of invalid 3rd party api response
		}
	}

	// Checks the token metadata endpoint for TarkovTracker
	private TeamProgressResponse GetTeamProgress()
	{
		try
		{
			var responseStr = APIClient.Get($"{TarkovTrackerUrl}/team/progress", _token.Id);
			return JsonConvert.DeserializeObject<TeamProgressResponse>(responseStr) ?? new();
		}
		catch (WebException e)
		{
			var status = (e.Response as HttpWebResponse)?.StatusCode;
			if (status is HttpStatusCode.TooManyRequests)
				throw new RateLimitExceededException("Rate Limiting reached for token", e);
			// Unknown error, continue throwing
			throw;
		}
	}

	// Checks the token metadata endpoint for TarkovTracker
	private ProgressResponse GetProgress()
	{
		try
		{
			var responseStr = APIClient.Get($"{TarkovTrackerUrl}/progress", _token.Id);
			return JsonConvert.DeserializeObject<ProgressResponse>(responseStr) ?? new();
		}
		catch (WebException e)
		{
			var status = (e.Response as HttpWebResponse)?.StatusCode;
			if (status is HttpStatusCode.TooManyRequests)
				throw new RateLimitExceededException("Rate Limiting reached for token", e);
			// Unknown error, continue throwing
			throw;
		}
	}

	public bool TestToken(string test_token)
	{
		try
		{
			GetToken(test_token);
		}
		catch (Exception)
		{
			return false;
		}

		return true;
	}

	// Checks the token metadata endpoint for TarkovTracker
	private TokenResponse? GetToken(string? custom_token = null)
	{
		var working_token = Token;
		if (custom_token != null) working_token = custom_token;

		try
		{
			var responseStr = APIClient.Get($"{TarkovTrackerUrl}/token", working_token);
			return JsonConvert.DeserializeObject<TokenResponse>(responseStr);
		}
		catch (WebException)
		{
			// Unknown error, continue throwing
			throw;
		}
	}
}
