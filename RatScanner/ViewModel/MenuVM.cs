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

namespace RatScanner.ViewModel
{
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
		public string Avg24hPrice => PriceToString(GetAvg24hPrice());

		private int GetAvg24hPrice()
		{
			return MatchedItem.GetAvg24hMarketPrice();
		}
		// ReSharper restore InconsistentNaming

		public string PricePerSlot => PriceToString(GetAvg24hPrice() / (MatchedItem.Width * MatchedItem.Height));

		public string TraderName => TraderPrice.GetTraderName(GetBestTrader().traderId);

		public string BestTraderPrice => IntToGroupedString(GetBestTrader().price) + " ₽";

		private (string traderId, int price) GetBestTrader()
		{
			return MatchedItem.GetBestTrader();
		}

		public string MaxTraderPrice => IntToGroupedString(GetMaxTraderPrice()) + " ₽";

		private int GetMaxTraderPrice()
		{
			return MatchedItem.GetMaxTraderPrice();
		}


		public NeededItem TrackingNeeds => MatchedItem.GetTrackingNeeds();
		public NeededItem TrackingTeamNeedsSummed => MatchedItem.GetSummedTrackingTeamNeeds();

		public string TrackingNeedsQuestRemaining => TrackingNeeds.QuestRemaining.ToString();
		public string TrackingNeedsHideoutRemaining => TrackingNeeds.QuestRemaining.ToString();

		public List<KeyValuePair<string, NeededItem>> TrackingTeamNeeds => MatchedItem.GetTrackingTeamNeeds();

		public List<KeyValuePair<string, NeededItem>> TrackingTeamNeedsFiltered => TrackingTeamNeeds?.Where(x => x.Value.Remaining > 0).ToList() ?? new List<KeyValuePair<string, NeededItem>>();

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

		public string ToolsLink => $"https://tarkov-tools.com/item/{ItemId}";

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

		private string PriceToString(int price) => IntToGroupedString(price) + " ₽";

		private static string IntToGroupedString(int? value)
		{
			if (value == null) return "ERROR";

			var text = $"{value:n0}";
			var numberGroupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
			return text.Replace(numberGroupSeparator, RatConfig.ToolTip.DigitGroupingSymbol);
		}
	}
}
