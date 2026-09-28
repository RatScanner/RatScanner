using RatScanner.Scan;
using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace RatScanner.ViewModel;

internal class MenuVM : INotifyPropertyChanged {
	public RatScannerMain DataSource {
		get;
		set {
			field = value;
			OnPropertyChanged();
		}
	}

	public ItemQueue ItemScans => DataSource.ItemScans;

	public ItemScan LastItemScan => ItemScans.LastOrDefault() ?? throw new Exception("ItemQueue is empty!");

	public Item LastItem => LastItemScan.Item;

	public static string DiscordLink => ApiManager.GetResource(ApiManager.ResourceType.DiscordLink);

	public static string GithubLink => ApiManager.GetResource(ApiManager.ResourceType.GithubLink);

	public static string PatreonLink => ApiManager.GetResource(ApiManager.ResourceType.PatreonLink);

	public string Updated => LastItem.Updated.ToString(CultureInfo.CurrentCulture);

	public string WikiLink {
		get {
			var link = LastItem.WikiLink;
			if (link?.Length > 3) return link;
			// Uri.EscapeDataString, not HttpUtility.UrlEncode: this value becomes a
			// path segment, where spaces must be percent-encoded (%20). UrlEncode is
			// form encoding and would emit "+", which is only meaningful in a query.
			return $"https://escapefromtarkov.gamepedia.com/{Uri.EscapeDataString(LastItem.Name.Replace(" ", "_"))}";
		}
	}

	public int PricePerSlot => LastItem.GetAvg24hMarketPricePerSlot();

	public TraderPrice? BestTraderOffer => LastItem.GetBestSellToTraderOffer();
	public TraderPrice? BestTraderOfferVendor => LastItem.GetBestSellToTraderOffer();

	public (int count, int kappaCount) TaskRemainingResult => LastItem.GetTaskRemaining();

	public int TaskRemaining => TaskRemainingResult.count;

	public int TaskRemainingKappa => TaskRemainingResult.kappaCount;

	public bool KappaNeeded => TaskRemainingKappa > 0;

	public int HideoutRemaining => LastItem.GetHideoutRemaining();

	public bool ItemNeeded => TaskRemaining + HideoutRemaining > 0;

	public static bool ShowKappaNeeds => RatConfig.Tracking.ShowKappaNeeds;

	public List<KeyValuePair<string, KeyValuePair<int, int>>>? ItemTeamNeeds {
		get {
			if (!RatConfig.Tracking.TarkovTracker.Enable) return null;
			var progress = RatScannerMain.Instance.TarkovTrackerDB.Progress;
			var teamProgress = progress.Where(x => x.UserId != RatScannerMain.Instance.TarkovTrackerDB.Self);

			List<KeyValuePair<string, KeyValuePair<int, int>>> needs = [];
			foreach (var memberProgress in teamProgress) {
				var task = LastItem.GetTaskRemaining(memberProgress).count;
				var hideout = LastItem.GetHideoutRemaining(memberProgress);

				if (task == 0 && hideout == 0) continue;

				KeyValuePair<int, int> need = new(task, hideout);

				var name = memberProgress.DisplayName ?? "Unknown";
				for (var i = 2; i < 99; i++) {
					if (needs.All(n => n.Key != name)) break;
					name = $"{memberProgress.DisplayName} #{i}";
				}

				needs.Add(new KeyValuePair<string, KeyValuePair<int, int>>(name, need));
			}

			return needs;
		}
	}

	public (int task, int hideout) ItemTeamNeedsSummed => (ItemTeamNeeds?.Sum(i => i.Value.Key) ?? 0, ItemTeamNeeds?.Sum(i => i.Value.Value) ?? 0);

	public bool ItemTeamNeeded => ItemTeamNeeds != null && ItemTeamNeeds.Count != 0;

	public event PropertyChangedEventHandler PropertyChanged;

	public MenuVM(RatScannerMain ratScanner) {
		DataSource = ratScanner;
		DataSource.PropertyChanged += ModelPropertyChanged;
	}

	protected virtual void OnPropertyChanged(string propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		OnPropertyChanged();
	}

	// Still used in minimal menu
	public static string IntToLongPrice(int? value) {
		if (value == null) return "0 ₽";

		var text = $"{value:n0}";
		var numberGroupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
		return text.Replace(numberGroupSeparator, RatConfig.ToolTip.DigitGroupingSymbol) + " ₽";
	}
}
