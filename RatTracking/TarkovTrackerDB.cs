using Newtonsoft.Json;
using RatTracking.FetchModels.TarkovTracker;
using System.Net;

namespace RatTracking;

// Storing information about progression from TarkovTracker API
public class TarkovTrackerDB
{
	private Token? _token;
	private bool _badToken;

	public List<Progress> Progress = new();
	public string? Token;

	private const string TarkovTrackerUrl = "https://tarkovtracker.io/api/v1";

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
			var newToken = getTarkovTrackerToken();
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
			_token = new Token { Id = Token };
		}
	}

	public int TeammateCount => Progress.Count(x => (x.Self ?? false) == false);

	public bool? TeamProgressAvailable => _token?.Permissions.Contains("TP");

	public bool? SoloProgressAvailable => _token?.Permissions.Contains("GP");

	private void UpdateProgression()
	{
		// We have access to team progression
		if (TeamProgressAvailable == true)
		{
			try
			{
				var rawProgress = JsonConvert.DeserializeObject<List<Progress>>(getTarkovTrackerTeam());
				Progress = rawProgress?.Where(x => (x.Hide ?? false) == false).ToList();
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
		else if (SoloProgressAvailable == true)
		{
			// We have permission to get individual progress

			try
			{
				var soloProgress = JsonConvert.DeserializeObject<Progress>(getTarkovTrackerSolo());
				Progress = new List<Progress> { soloProgress };
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
		else
		{
			// We dont have permissions
		}
	}

	// Checks the token metadata endpoint for TarkovTracker
	private string? getTarkovTrackerTeam()
	{
		try
		{
			return APIClient.Get($"{TarkovTrackerUrl}/team/progress", _token.Id);
		}
		catch (WebException e)
		{
			var status = (e.Response as HttpWebResponse)?.StatusCode;
			if (status is HttpStatusCode.TooManyRequests)
				throw new RateLimitExceededException("Rate Limiting reached for token", e);
			// Unknown error, continue throwing
			throw;
		}
		catch (Exception)
		{
			return null;
		}
	}

	// Checks the token metadata endpoint for TarkovTracker
	private string getTarkovTrackerSolo()
	{
		try
		{
			return APIClient.Get($"{TarkovTrackerUrl}/progress", _token.Id);
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
			getTarkovTrackerToken(test_token);
		}
		catch (Exception)
		{
			return false;
		}

		return true;
	}

	// Checks the token metadata endpoint for TarkovTracker
	private Token? getTarkovTrackerToken(string custom_token = null)
	{
		var working_token = Token;
		if (custom_token != null) working_token = custom_token;

		try
		{
			var response = APIClient.Get($"{TarkovTrackerUrl}/token", working_token);
			if (response == null) return null;
			try
			{
				return JsonConvert.DeserializeObject<Token>(response);
			}
			catch (Exception)
			{
				return null;
			}
		}
		catch (WebException)
		{
			// Unknown error, continue throwing
			throw;
		}
	}
}
