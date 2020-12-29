using System;
using System.ComponentModel;
using System.Globalization;
using RatScanner.FetchModels;
using RatScanner.Scan;

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

		private MarketItem[] MatchedItems => CurrentItemScan?.MatchedItems;

		public string IconPath => IconManager.GetIconPath(MatchedItems[0]);

		public string Name => MatchedItems[0].Name;

		public bool HasMods => MatchedItems[0].HasMods;

		public string Price => PriceToString(GetPrice());

		private int GetPrice()
		{
			return MatchedItems[0].SumMods(item => item.Price);
		}

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable InconsistentNaming
		public string Avg24hPrice => PriceToString(GetAvg24hPrice());

		private int GetAvg24hPrice()
		{
			return MatchedItems[0].SumMods(item => item.Avg24hPrice);
		}
		// ReSharper restore InconsistentNaming

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable InconsistentNaming
		public string Avg7dPrice => PriceToString(GetAvg24hPrice());

		private int GetAvg7dPrice()
		{
			return MatchedItems[0].SumMods(item => item.Avg7dPrice);
		}
		// ReSharper restore InconsistentNaming

		public string PricePerSlot => PriceToString(GetAvg24hPrice() / MatchedItems[0].Slots);

		public string TraderName => MatchedItems[0].TraderName;

		public string TraderPrice
		{
			get
			{
				var currency = CurrentItemScan.MatchedItems[0].TraderCurrency;
				return IntToGroupedString(GetTraderPrice()) + " " + currency;
			}
		}

		private int GetTraderPrice()
		{
			// TODO do not mix currency's
			return MatchedItems[0].SumMods(item => item.TraderPrice);
		}

		public string QuestCount => ""+MatchedItems[0].QuestCount;
		public string QuestInRaid => "" + MatchedItems[0].QuestInRaid;

		public string DiscordLink => ApiManager.GetResource(ApiManager.ResourceType.Discord);

		public string GithubLink => ApiManager.GetResource(ApiManager.ResourceType.Github);

		public string PatreonLink => ApiManager.GetResource(ApiManager.ResourceType.Patreon);

		public string Updated
		{
			get
			{
				var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				var min = MatchedItems[0].MinMods(item => item.Timestamp);
				return dt.AddSeconds(min).ToLocalTime().ToString(CultureInfo.CurrentCulture);
			}
		}

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable once InconsistentNaming
		public string Diff24h
		{
			get
			{
				var indicator = Diff24hPositive ? UpSymbol : DownSymbol;
				var diff = (double)(GetAvg24hPrice() - GetAvg24hAgo()) / GetAvg24hAgo();
				return diff.ToString("N2") + " " + indicator;
			}
		}

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable InconsistentNaming
		public string Avg24hAgo => PriceToString(GetAvg24hAgo());

		private int GetAvg24hAgo()
		{
			return MatchedItems[0].SumMods(item => item.Avg24hAgo);
		}
		// ReSharper restore InconsistentNaming

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable once InconsistentNaming
		public bool Diff24hPositive => GetAvg24hPrice() - GetAvg24hAgo() >= 0;

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable once InconsistentNaming
		public string Diff7d
		{
			get
			{
				var indicator = Diff7dPositive ? UpSymbol : DownSymbol;
				var diff = (double)(GetAvg24hPrice() - GetAvg7dAgo()) / GetAvg7dAgo();
				return diff.ToString("N2") + " " + indicator;
			}
		}

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable InconsistentNaming
		public string Avg7dAgo => PriceToString(GetAvg7dAgo());

		private int GetAvg7dAgo()
		{
			return MatchedItems[0].SumMods(item => item.Avg7dAgo);
		}
		// ReSharper restore InconsistentNaming

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable once InconsistentNaming
		public bool Diff7dPositive => GetAvg7dPrice() - GetAvg7dAgo() >= 0;

		public string WikiLink => MatchedItems[0].WikiLink;

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
