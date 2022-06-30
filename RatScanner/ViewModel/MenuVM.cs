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
using RatLib.Scan;

namespace RatScanner.ViewModel;

internal class MainWindowVM : INotifyPropertyChanged, IRatScannerUI
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

	public ItemScan CurrentItemScan => DataSource?.CurrentItemScan;

	private Item MatchedItem => CurrentItemScan?.MatchedItem;

	public string ItemId => MatchedItem.Id;

	public string IconPath
	{
		get
		{
			ItemExtraInfo itemExtraInfo;
			if (CurrentItemScan is ItemIconScan scan) itemExtraInfo = scan.ItemExtraInfo;
			else itemExtraInfo = new ItemExtraInfo();
			var path = CurrentItemScan.IconPath;
			return path ?? RatConfig.Paths.UnknownIcon;
		}
	}

	public string Name => MatchedItem.Name;

	public string ShortName => MatchedItem.ShortName;

	public bool HasMods => MatchedItem is CompoundItem itemC && itemC.Slots.Count > 0;

	// https://youtrack.jetbrains.com/issue/RSRP-468572
	// ReSharper disable InconsistentNaming
	public int Avg24hPrice => MatchedItem.GetAvg24hMarketPrice();
	// ReSharper restore InconsistentNaming

	public int PricePerSlot => MatchedItem.GetAvg24hMarketPrice() / (MatchedItem.Width * MatchedItem.Height);

	public string TraderName => TraderPrice.GetTraderName(MatchedItem.GetBestTrader().traderId);

	public int BestTraderPrice => MatchedItem.GetBestTrader().price;

	public int MaxTraderPrice => MatchedItem.GetMaxTraderPrice();


	public NeededItem TrackingNeeds => MatchedItem.GetTrackingNeeds();
	public NeededItem TrackingTeamNeedsSummed => MatchedItem.GetSummedTrackingTeamNeeds();

	public string TrackingNeedsQuestRemaining => TrackingNeeds.QuestRemaining.ToString();
	public string TrackingNeedsHideoutRemaining => TrackingNeeds.QuestRemaining.ToString();

	public List<KeyValuePair<string, NeededItem>> TrackingTeamNeeds => MatchedItem.GetTrackingTeamNeeds();

	public List<KeyValuePair<string, NeededItem>> TrackingTeamNeedsFiltered =>
		TrackingTeamNeeds?.Where(x => x.Value.Remaining > 0).ToList() ?? new List<KeyValuePair<string, NeededItem>>();

	public string DiscordLink => ApiManager.GetResource(ApiManager.ResourceType.DiscordLink);

	public string GithubLink => ApiManager.GetResource(ApiManager.ResourceType.GithubLink);

	public string PatreonLink => ApiManager.GetResource(ApiManager.ResourceType.PatreonLink);

	public string Updated
	{
		get
		{
			var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var min = MatchedItem.GetMarketItem().Timestamp;
			return dt.AddSeconds(min).ToLocalTime().ToString(CultureInfo.CurrentCulture);
		}
	}

	public string WikiLink
	{
		get
		{
			var link = MatchedItem.GetMarketItem().WikiLink;
			if (link.Length > 3) return link;
			return $"https://escapefromtarkov.gamepedia.com/{HttpUtility.UrlEncode(Name.Replace(" ", "_"))}";
		}
	}

	public string TarkovDevLink => $"https://tarkov.dev/item/{ItemId}";

	public string IconLink => MatchedItem.GetMarketItem().IconLink;
	public string ImageLink => MatchedItem.GetMarketItem().ImageLink;

	public event PropertyChangedEventHandler PropertyChanged;

	public MainWindowVM(RatScannerMain ratScanner)
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

		var result = priceStr.Substring(0, 3);

		var dotPos = priceStr.Length % 3;
		//if (dotPos != 0) result = result.Insert(dotPos, ".");
		if (dotPos != 0) result = result[..dotPos];

		result += " " + suffixes[(int)Math.Floor((priceStr.Length - 1) / 3f)];
		return "₽ " + result;
	}

	private static (int need, int have, bool FIR) GetQuestRequired(IEnumerable<QuestItem> requiredQuestItems, Progress progress)
	{
		var need = 0;
		var have = 0;
		var fir = false;

		// Add up all the quest requirements
		foreach (var requirement in requiredQuestItems)
			// Add the item if its FIR or we want to show non FIR
			if (requirement.FIR || RatConfig.Tracking.ShowNonFIRNeeds)
			{
				// Update FIR flag to true if any quest is FIR
				if (requirement.FIR) fir = true;

				if (progress == null || progress.QuestObjectives == null)
				{
					need += requirement.Needed;
					continue;
				}

				// If the progress data doesn't have this requirement, then it should be a needed item
				if (!progress.QuestObjectives.ContainsKey(requirement.QuestObjectiveId.ToString()))
				{
					need += requirement.Needed;
				}
				// Else if we have the requirement in our progress data but its not complete, it might have metadata
				else if (progress.QuestObjectives[requirement.QuestObjectiveId.ToString()].Complete != true)
				{
					// Check if we have completed this quest need
					need += requirement.Needed;
					have += progress.QuestObjectives[requirement.QuestObjectiveId.ToString()].Have ?? 0;
				}
			}

		return (need, have, fir);
	}

	private static (int need, int have) GetHideoutRequired(IEnumerable<HideoutItem> requiredHideoutItems, Progress progress)
	{
		var need = 0;
		var have = 0;

		// Add up all the hideout requirements
		foreach (var requirement in requiredHideoutItems)
		{
			if (progress == null || progress.HideoutObjectives == null)
			{
				need += requirement.Needed;
				continue;
			}

			// If the progress data doesn't have this requirement, then it should be a needed item
			if (!progress.HideoutObjectives.ContainsKey(requirement.HideoutObjectiveId.ToString()))
			{
				need += requirement.Needed;
			}
			// Else if we have the requirement in our progress data but its not complete, it might have metadata
			else if (progress.HideoutObjectives[requirement.HideoutObjectiveId.ToString()].Complete != true)
			{
				// Check if we have completed this hideout need
				need += requirement.Needed;
				have += progress.HideoutObjectives[requirement.HideoutObjectiveId.ToString()].Have ?? 0;
			}
		}

		return (need, have);
	}
}
