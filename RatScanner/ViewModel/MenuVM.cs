using System;
using System.ComponentModel;
using System.Globalization;
using RatScanner.FetchModels;
using RatScanner.Scan;
using RatStash;

namespace RatScanner.ViewModel
{
	internal class MainWindowVM : INotifyPropertyChanged
	{
		public const string UpSymbol = "▲";
		public const string DownSymbol = "▼";

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

		private bool wishlistChanged;

		private ItemScan CurrentItemScan => DataSource?.CurrentItemScan;

		private Item[] MatchedItems => CurrentItemScan?.MatchedItems;

		private Item currentItem;
		private Item CurrentItem
		{
			get
			{
				if (currentItem != MatchedItems[0])
				{
					if (wishlistChanged)
					{
						RatScannerMain.Instance.SaveWishlist();
					}

					currentItem = MatchedItems[0];
					wishlistChanged = false;
				}

				return currentItem;
			}
		}

		public string IconPath => IconManager.GetIconPath(CurrentItem);

		public string Name => CurrentItem.Name;

		public bool HasMods => CurrentItem is CompoundItem itemC && itemC.Slots.Count > 0;

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable InconsistentNaming
		public string Avg24hPrice => PriceToString(GetAvg24hPrice());

		private int GetAvg24hPrice() => CurrentItem.GetAvg24hMarketPrice();
		// ReSharper restore InconsistentNaming

		public string PricePerSlot => PriceToString(GetAvg24hPrice() / (CurrentItem.Width * CurrentItem.Height));

		public string TraderName => TraderPrice.GetTraderName(GetBestTrader().traderId);

		public string BestTraderPrice => IntToGroupedString(GetBestTrader().price) + " ₽";

		private (string traderId, int price) GetBestTrader() => CurrentItem.GetBestTrader();

		public string MaxTraderPrice => IntToGroupedString(GetMaxTraderPrice()) + " ₽";

		private int GetMaxTraderPrice() => CurrentItem.GetMaxTraderPrice();

		public string DiscordLink => ApiManager.GetResource(ApiManager.ResourceType.DiscordLink);

		public string GithubLink => ApiManager.GetResource(ApiManager.ResourceType.GithubLink);

		public string PatreonLink => ApiManager.GetResource(ApiManager.ResourceType.PatreonLink);

		public string Updated
		{
			get
			{
				var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				var min = CurrentItem.GetMarketItem().Timestamp;
				return dt.AddSeconds(min).ToLocalTime().ToString(CultureInfo.CurrentCulture);
			}
		}

		public int WishlistAmount => CurrentItem.GetWishlistAmount();

		public string WikiLink => CurrentItem.GetMarketItem().WikiLink;

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

		#region Commands
		private readonly RelayCommand increaseAmountCommand;
		public RelayCommand IncreaseAmountCommand
		{
			get
			{
				return increaseAmountCommand ?? new RelayCommand(() => IncreaseAmount());
			}
		}

		private readonly RelayCommand decreaseAmountCommand;
		public RelayCommand DecreaseAmountCommand
		{
			get
			{
				return decreaseAmountCommand ?? new RelayCommand(() => DecreaseAmount(), (wa) => WishlistAmount != 0);
			}
		}
		#endregion

		#region Methods
		private void IncreaseAmount()
		{
			CurrentItem.SetWishlistAmount(WishlistAmount + 1);
			wishlistChanged = true;

			OnPropertyChanged("WishlistAmount");
			OnPropertyChanged("DecreaseAmountCommand");
		}

		private void DecreaseAmount()
		{
			CurrentItem.SetWishlistAmount(WishlistAmount - 1);
			wishlistChanged = true;
			OnPropertyChanged("WishlistAmount");
		}
		#endregion

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
