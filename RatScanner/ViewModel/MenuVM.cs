using RatRazor.Interfaces;
using RatScanner.FetchModels;
using RatStash;
using RatTracking.FetchModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using RatTracking;
using RatLib.Scan;
using RatTracking.FetchModels.TarkovTracker;

namespace RatScanner.ViewModel;

internal class MenuVM : INotifyPropertyChanged, IRatScannerUI
{
	private const string UpSymbol = "▲";
	private const string DownSymbol = "▼";

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

	public RatStash.Database RatStashDB => DataSource?.RatStashDB;

	public TarkovTrackerDB TarkovTrackerDB => DataSource?.TarkovTrackerDB;

	public ProgressDB ProgressDB => DataSource?.ProgressDB;

	public ItemQueue ItemScans => DataSource?.ItemScans;

	public ItemScan LastItemScan => ItemScans.Last();

	public Item LastItem => LastItemScan.MatchedItem;

	public NeededItem TrackingNeeds => LastItem.GetTrackingNeeds();
	public NeededItem TrackingTeamNeedsSummed => LastItem.GetSummedTrackingTeamNeeds();

	public int TrackingNeedsQuestRemaining => TrackingNeeds.QuestRemaining;
	public int TrackingNeedsHideoutRemaining => TrackingNeeds.QuestRemaining;

	public List<KeyValuePair<string, NeededItem>> TrackingTeamNeeds => LastItem.GetTrackingTeamNeeds();

	public List<KeyValuePair<string, NeededItem>> TrackingTeamNeedsFiltered =>
		TrackingTeamNeeds?.Where(x => x.Value.Remaining > 0).ToList() ?? new List<KeyValuePair<string, NeededItem>>();

	public NeededItem GetItemNeeds(ItemScan itemScan)
	{
		var item = itemScan.MatchedItem;
		var requiredQuest = item.GetQuestRequired();
		var requiredHideout = item.GetHideoutRequired();

		var neededItem = new NeededItem(item.Id);

		UserProgress progress = null;
		if (RatConfig.Tracking.TarkovTracker.Enable && RatScannerMain.Instance.TarkovTrackerDB.Progress.Count >= 1)
		{
			var teamProgress = RatScannerMain.Instance.TarkovTrackerDB.Progress;
			progress = teamProgress.FirstOrDefault(x => x.UserId == RatScannerMain.Instance.TarkovTrackerDB.Self);
		}

		// Set this item as for ourselves if this was our token
		neededItem.Self = true;

		var (questNeeded, questHave, fir) = GetQuestRequired(requiredQuest, progress);
		neededItem.QuestNeeded += questNeeded;
		neededItem.QuestHave += questHave;
		neededItem.FIR = fir;

		var (hideoutNeeded, hideoutHave) = GetHideoutRequired(requiredHideout, progress);
		neededItem.HideoutNeeded += hideoutNeeded;
		neededItem.HideoutHave += hideoutHave;
		return neededItem;
	}

	public List<KeyValuePair<string, NeededItem>> GetItemTeamNeeds(ItemScan itemScan)
	{
		if (!RatConfig.Tracking.TarkovTracker.Enable) return null;

		var item = itemScan.MatchedItem;

		var requiredQuest = item.GetQuestRequired();
		var requiredHideout = item.GetHideoutRequired();

		var teamProgress = RatScannerMain.Instance.TarkovTrackerDB.Progress;

		var trackedNeeds = new Dictionary<string, NeededItem>();

		foreach (var memberProgress in teamProgress)
		{
			var neededItem = new NeededItem(item.Id);
			var (questNeeded, questHave, fir) = GetQuestRequired(requiredQuest, memberProgress);
			neededItem.QuestNeeded += questNeeded;
			neededItem.QuestHave += questHave;
			neededItem.FIR = fir;

			var (hideoutNeeded, hideoutHave) = GetHideoutRequired(requiredHideout, memberProgress);
			neededItem.HideoutNeeded += hideoutNeeded;
			neededItem.HideoutHave += hideoutHave;

			var name = memberProgress.DisplayName ?? "Unknown";
			for (var i = 2; i < 99; i++)
			{
				if (!trackedNeeds.ContainsKey(name)) break;
				name = $"{memberProgress.DisplayName} #{i}";
			}

			trackedNeeds.Add(name, neededItem);
		}

		return trackedNeeds.OrderBy(x => -x.Value.Remaining).ToList();
	}

	public NeededItem GetItemTeamNeedsSummed(ItemScan itemScan)
	{
		var item = itemScan.MatchedItem;

		var result = new NeededItem(item.Id);
		var teamData = item.GetTrackingTeamNeeds();
		if (teamData == null)
		{
			result.HideoutNeeded = 0;
			result.QuestNeeded = 0;
			result.HideoutHave = 0;
			result.QuestHave = 0;
			return result;
		}

		foreach (var (_, value) in teamData)
		{
			result.HideoutNeeded += value.HideoutNeeded;
			result.QuestNeeded += value.QuestNeeded;
			result.HideoutHave += value.HideoutHave;
			result.QuestHave += value.QuestHave;
		}

		return result;
	}

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

	public string IntToLongPrice(int? value)
	{
		if (value == null) return "0 ₽";

		var text = $"{value:n0}";
		var numberGroupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
		return text.Replace(numberGroupSeparator, RatConfig.ToolTip.DigitGroupingSymbol) + " ₽";
	}

	public string IntToShortPrice(int? value)
	{
		if (value == null) return "₽ 0";

		var priceStr = value.ToString();
		if (priceStr.Length < 4) return "₽ " + priceStr;

		var suffixes = new string[] { "", "K", "M", "B", "T" };

		var result = priceStr[..3];

		var dotPos = priceStr.Length % 3;
		//if (dotPos != 0) result = result.Insert(dotPos, ".");
		if (dotPos != 0) result = result[..dotPos];

		result += " " + suffixes[(int)Math.Floor((priceStr.Length - 1) / 3f)];
		return "₽ " + result;
	}

	private static (int need, int have, bool FIR) GetQuestRequired(IEnumerable<QuestItem> requiredQuestItems, UserProgress progress)
	{
		var need = 0;
		var have = 0;
		var fir = false;

		// Add up all the quest requirements
		foreach (var requirement in requiredQuestItems)
		{
			// Add the item if its FIR or we want to show non FIR
			if (requirement.FIR || RatConfig.Tracking.ShowNonFIRNeeds)
			{
				// Update FIR flag to true if any quest is FIR
				if (requirement.FIR) fir = true;

				if (progress == null || progress.TaskObjectives == null)
				{
					need += requirement.Needed;
					continue;
				}

				// Check if the requirement task objective id exists in progress.TaskObjectives
				var taskObjective = progress.TaskObjectives.Find(x => x.Id == requirement.TaskObjectiveId);
				if (taskObjective == null)
				{
					need += requirement.Needed;
				}
				else
				{
					if (taskObjective.Complete)
					{
						have += requirement.Needed;
					}
					else
					{
						have += taskObjective.Count;
					}
					need += requirement.Needed;

				}
			}
		}

		return (need, have, fir);
	}

	private static (int need, int have) GetHideoutRequired(IEnumerable<HideoutItem> requiredHideoutItems, UserProgress progress)
	{
		var need = 0;
		var have = 0;

		// Add up all the hideout requirements
		foreach (var requirement in requiredHideoutItems)
		{
			if (progress == null || progress.HideoutParts == null)
			{
				need += requirement.Needed;
				continue;
			}

			// Check if the requirement hideout part id exists in progress.HideoutParts
			var hideoutPart = progress.HideoutParts.Find(x => x.Id == requirement.HideoutPartId);
			if (hideoutPart == null)
			{
				need += requirement.Needed;
			}
			else
			{
				need += requirement.Needed;
				if (hideoutPart.Complete)
				{
					have += requirement.Needed;
				}
				else
				{
					have += hideoutPart.Count;
				}
			}
		}

		return (need, have);
	}
}
