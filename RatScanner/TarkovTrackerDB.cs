using Newtonsoft.Json;
using RatScanner.FetchModels.TarkovTracker;
using System;
using System.Collections.Generic;

namespace RatScanner
{
	// Storing information about progression from TarkovTracker API
	public class TarkovTrackerDB
	{

		private Token? _token;
		private bool _badToken = false;

		private List<Progress> _progress;

		private DateTime? _lastUpdate;

		// Set up the TarkovTracker DB
		public void Init()
		{
			UpdateProgression();
		}

		public List<Progress> GetProgress()
		{
			UpdateProgression();
			return _progress;
		}

		public bool ValidToken()
		{
			if (RatConfig.Tracking.TarkovTracker.Token.Length > 0)
			{
				// We have a Token in our config, check it
				if (_token != null)
				{
					if (_token.Id != RatConfig.Tracking.TarkovTracker.Token)
					{
						// Token in config versus last attempted token are different
						updateToken();
					}
				}
				else
				{
					// We have a token config, but haven't tried validating the token
					updateToken();
				}
				return !_badToken;
			}
			else
			{
				return false;
			}
		}

		private void updateToken()
		{
			// Attempt to verify the token
			var response = ApiManager.GetTarkovTrackerToken();
			if (response != null)
			{
				// We have a valid token
				_token = JsonConvert.DeserializeObject<Token>(response);
				_badToken = false;
			}
			else
			{
				// We have a token that doesn't work, mark it as such
				_badToken = true;
				_token = new Token { Id = RatConfig.Tracking.TarkovTracker.Token };
			}
		}

		private void updateDisplayNames()
		{
			int personNum = 1;
			foreach (Progress teammate in _progress)
			{
				if (teammate.DisplayName == null)
				{
					teammate.DisplayName = "Unknown #" + personNum;
				}
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

		public void UpdateProgression()
		{
			if(ValidToken())
			{
				// If we last updated more than 10 minutes ago, update again
				if(_lastUpdate == null || (_lastUpdate - DateTime.Now) < TimeSpan.FromMinutes(10))
				{
					// We have access to team progression
					if (TeamProgressAvailable())
					{
						_progress = JsonConvert.DeserializeObject<List<Progress>>(ApiManager.GetTarkovTrackerTeam());
						_lastUpdate = DateTime.Now;
						updateDisplayNames();
					}
					// We have permission to get individual progress
					else if (SoloProgressAvailable())
					{
						var soloProgress = JsonConvert.DeserializeObject<Progress>(ApiManager.GetTarkovTrackerSolo());
						_progress = new List<Progress>();
						_progress.Add(soloProgress);
						_lastUpdate = DateTime.Now;
						updateDisplayNames();
					}
					else
					{
						// We dont have any supported permissions to get progression :(
						_lastUpdate = DateTime.Now;
					}
				}
			}
		}

	}
}
