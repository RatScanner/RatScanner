using Newtonsoft.Json;
using RatScanner.FetchModels.TarkovTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;

namespace RatScanner
{
	// Storing information about progression from TarkovTracker API
	public class TarkovTrackerDB
	{
		private Token _token;
		private bool _badToken;

		public List<Progress> Progress = new List<Progress>();

		// Set up the TarkovTracker DB
		public bool Init()
		{
			if (!ValidToken()) return false;
			UpdateProgression();
			return true;
		}

		private bool ValidToken()
		{
			if (RatConfig.Tracking.TarkovTracker.Token.Length > 0)
			{
				// We have a Token in our config, check it
				if (_token != null)
				{
					if (_token.Id != RatConfig.Tracking.TarkovTracker.Token)
					{
						// Token in config versus last attempted token are different
						UpdateToken();
					}
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
				var response = ApiManager.GetTarkovTrackerToken();
				if (response != null)
				{
					// We have a valid token
					_token = JsonConvert.DeserializeObject<Token>(response);
					_badToken = false;
				}
				else
				{
					Logger.LogWarning("TarkovTracker token refresh responded with null!");
					_badToken = true;
				}
			}
			catch (RateLimitExceededException)
			{
				// We hit a rate limit issue, this doesn't mean our token is bad, but we have to wait until we try again
				Logger.LogWarning("TarkovTracker rate limit reached!");
			}
			catch (UnauthorizedTokenException)
			{
				// We have an unauthorized token, retrying won't help until we change it
				Logger.LogWarning("Unauthorized TarkovTracker token!");
				_badToken = true;
				_token = new Token { Id = RatConfig.Tracking.TarkovTracker.Token };
			}
		}

		public int TeammateCount
		{
			get
			{
				// Are we set to utilize teams?
				if (RatConfig.Tracking.TarkovTracker.ShowTeam) return Math.Max(Progress.Count - 1, 0);

				// We are only supposed to show solo, so just count 1 if we have any progress
				return Progress.Count > 0 ? 1 : 0;
			}
		}

		public bool TeamProgressAvailable()
		{
			return _token.Permissions.Contains("TP");
		}

		public bool SoloProgressAvailable()
		{
			return _token.Permissions.Contains("GP");
		}

		private void UpdateProgression()
		{
			// We have access to team progression
			if (TeamProgressAvailable())
				try
				{
					Progress = JsonConvert.DeserializeObject<List<Progress>>(ApiManager.GetTarkovTrackerTeam());
				}
				catch (RateLimitExceededException e)
				{
					// We hit a rate limit issue, this doesn't mean our token is bad, but we have to wait until we try again
					Logger.LogWarning(e.Message);
				}
				catch (UnauthorizedTokenException e)
				{
					// We have an unauthorized token exception, it could be that we don't have permissions for this call
					Logger.LogWarning(e.Message);
				}
			// We have permission to get individual progress
			else if (SoloProgressAvailable())
				try
				{
					var soloProgress = JsonConvert.DeserializeObject<Progress>(ApiManager.GetTarkovTrackerSolo());
					Progress = new List<Progress> { soloProgress };
				}
				catch (RateLimitExceededException e)
				{
					// We hit a rate limit issue, this doesn't mean our token is bad, but we have to wait until we try again
					Logger.LogWarning(e.Message);
				}
				catch (UnauthorizedTokenException e)
				{
					// We have an unauthorized token exception, it could be that we don't have permissions for this call
					Logger.LogWarning(e.Message);
				}
			else
				Logger.ShowWarning("This TarkovTracker API Token has insufficient permissions.");
		}
	}
}
