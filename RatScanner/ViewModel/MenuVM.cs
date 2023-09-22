using RatScanner.FetchModels.TarkovTracker;
using RatScanner.Scan;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;

namespace RatScanner.ViewModel;

internal class MenuVM : INotifyPropertyChanged
{
	private RatScannerMain _dataSource;

	public RatScannerMain DataSource
	{
		get => _dataSource;
		set
		{
			_dataSource = value;
			OnPropertyChanged();
		}
	}

	public ItemQueue ItemScans => DataSource?.ItemScans;

	public ItemScan LastItemScan => ItemScans.LastOrDefault(new DefaultItemScan(true));

	public RatStash.Item LastItem => LastItemScan.MatchedItem;

	public string DiscordLink => ApiManager.GetResource(ApiManager.ResourceType.DiscordLink);

	public string GithubLink => ApiManager.GetResource(ApiManager.ResourceType.GithubLink);

	public string PatreonLink => ApiManager.GetResource(ApiManager.ResourceType.PatreonLink);

	public string Updated
	{
		get
		{
			var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var min = LastItem.GetMarketItem().Timestamp;
			return dt.AddSeconds(min).ToLocalTime().ToString(CultureInfo.CurrentCulture);
		}
	}

	public string WikiLink
	{
		get
		{
			var link = LastItem.GetMarketItem().WikiLink;
			if (link.Length > 3) return link;
			return $"https://escapefromtarkov.gamepedia.com/{HttpUtility.UrlEncode(LastItem.Name.Replace(" ", "_"))}";
		}
	}

	private UserProgress GetUserProgress()
	{
		UserProgress progress = null;
		if (RatConfig.Tracking.TarkovTracker.Enable && RatScannerMain.Instance.TarkovTrackerDB.Progress.Count >= 1)
		{
			var teamProgress = RatScannerMain.Instance.TarkovTrackerDB.Progress;
			progress = teamProgress.FirstOrDefault(x => x.UserId == RatScannerMain.Instance.TarkovTrackerDB.Self);
		}
		return progress;
	}

	public int TaskRemaining => LastItem.GetTaskRemaining(!RatConfig.Tracking.ShowNonFIRNeeds, GetUserProgress());

	public int HideoutRemaining => LastItem.GetHideoutRemaining(false, GetUserProgress());

	public bool ItemNeeded => TaskRemaining + HideoutRemaining > 0;

	public List<KeyValuePair<string, KeyValuePair<int, int>>> ItemTeamNeeds
	{
		get
		{
			if (!RatConfig.Tracking.TarkovTracker.Enable) return null;
			var progress = RatScannerMain.Instance.TarkovTrackerDB.Progress;
			var teamProgress = progress.Where(x => x.UserId != RatScannerMain.Instance.TarkovTrackerDB.Self);

			var needs = new List<KeyValuePair<string, KeyValuePair<int, int>>>();
			foreach (var memberProgress in teamProgress)
			{
				var task = LastItem.GetTaskRemaining(!RatConfig.Tracking.ShowNonFIRNeeds, memberProgress);
				var hideout = LastItem.GetHideoutRemaining(false, memberProgress);

				if (task == 0 && hideout == 0) continue;

				var need = new KeyValuePair<int, int>(task, hideout);

				var name = memberProgress.DisplayName ?? "Unknown";
				for (var i = 2; i < 99; i++)
				{
					if (needs.All(n => n.Key != name)) break;
					name = $"{memberProgress.DisplayName} #{i}";
				}

				needs.Add(new KeyValuePair<string, KeyValuePair<int, int>>(name, need));
			}

			return needs;
		}
	}

	public (int task, int hideout) ItemTeamNeedsSummed => (ItemTeamNeeds.Sum(i => i.Value.Key), ItemTeamNeeds.Sum(i => i.Value.Value));

	public bool ItemTeamNeeded => ItemTeamNeeds != null && ItemTeamNeeds.Any();

	public event PropertyChangedEventHandler PropertyChanged;

	public MenuVM(RatScannerMain ratScanner)
	{
		DataSource = ratScanner;
		DataSource.PropertyChanged += ModelPropertyChanged;
	}

	protected virtual void OnPropertyChanged(string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		OnPropertyChanged();
	}

	// Still used in minimal menu
	public string IntToLongPrice(int? value)
	{
		if (value == null) return "0 ₽";

		var text = $"{value:n0}";
		var numberGroupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
		return text.Replace(numberGroupSeparator, RatConfig.ToolTip.DigitGroupingSymbol) + " ₽";
	}
}
