using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using RatScanner.FetchModels;
using RatScanner.Scan;
using RatStash;

namespace RatScanner.ViewModel
{
	internal class MainWindowVM : INotifyPropertyChanged
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

		private ItemScan CurrentItemScan => DataSource?.CurrentItemScan;

		private Item[] MatchedItems => CurrentItemScan?.MatchedItems;

		public string IconPath => IconManager.GetIconPath(MatchedItems[0]);

		public string Name => MatchedItems[0].Name;

		public bool HasMods => MatchedItems[0] is CompoundItem itemC && itemC.Slots.Count > 0;

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable InconsistentNaming
		public string Avg24hPrice => PriceToString(GetAvg24hPrice());

		private int GetAvg24hPrice() => MatchedItems[0].GetAvg24hMarketPrice();
		// ReSharper restore InconsistentNaming

		public string PricePerSlot => PriceToString(GetAvg24hPrice() / (MatchedItems[0].Width * MatchedItems[0].Height));

		public string TraderName => TraderPrice.GetTraderName(GetBestTrader().traderId);

		public string BestTraderPrice => IntToGroupedString(GetBestTrader().price) + " ₽";

		private (string traderId, int price) GetBestTrader() => MatchedItems[0].GetBestTrader();

		public string MaxTraderPrice => IntToGroupedString(GetMaxTraderPrice()) + " ₽";

		private int GetMaxTraderPrice() => MatchedItems[0].GetMaxTraderPrice();

		public Dictionary<string, NeededItem> TrackingNeeds => MatchedItems[0].GetTrackingNeeds();

		public string DiscordLink => ApiManager.GetResource(ApiManager.ResourceType.DiscordLink);

		public string GithubLink => ApiManager.GetResource(ApiManager.ResourceType.GithubLink);

		public string PatreonLink => ApiManager.GetResource(ApiManager.ResourceType.PatreonLink);

		public string Updated
		{
			get
			{
				var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				var min = MatchedItems[0].GetMarketItem().Timestamp;
				return dt.AddSeconds(min).ToLocalTime().ToString(CultureInfo.CurrentCulture);
			}
		}

		public string WikiLink => MatchedItems[0].GetMarketItem().WikiLink;

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

		private string PriceToString(int price)
		{
			if (MatchedItems.Length == 1)
			{
				return IntToGroupedString(price) + " ₽";
			}

			// TODO make this more informative. Perhaps a value range?
			return "Uncertain";
		}

		private static string IntToGroupedString(int? value)
		{
			if (value == null) return "ERROR";

			var text = $"{value:n0}";
			var numberGroupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
			return text.Replace(numberGroupSeparator, RatConfig.ToolTip.DigitGroupingSymbol);
		}
	}
}
