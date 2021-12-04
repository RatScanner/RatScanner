using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RatScanner.Scan;
using RatRazor.Interfaces;
using RatScanner.FetchModels;
using RatStash;
using System.Globalization;
using System.ComponentModel;

namespace RatScanner.ViewModel
{
	public class BlazorVM : INotifyPropertyChanged
	{
		// Blazor ViewModel Constructor
		public BlazorVM() {
			_dataSource = RatScannerMain.Instance;
			_dataSource.PropertyChanged += ModelPropertyChanged;
		}

		// What we'll use as our primary data source
		private readonly RatScannerMain _dataSource;

		private ItemScan CurrentItemScan => _dataSource.CurrentItemScan;

		private Item[] MatchedItems => CurrentItemScan.MatchedItems;

		// Interface property for current scan's matched item name
		public string CurrentItemName => MatchedItems[0].Name;

		// The path to the icon for the current scan 
		public string IconPath
		{
			get
			{
				var path = CurrentItemScan.IconPath;
				return path ?? RatConfig.Paths.UnknownIcon;
			}
		}

		public string AverageDayPrice => PriceToString(_avg24hPrice);
		public string SlotValue => PriceToString(_avg24hPrice / (MatchedItems[0].Width * MatchedItems[0].Height));
		public string BestTraderPrice => IntToGroupedString(GetBestTrader().price) + " ₽";
		public string BestTrader => TraderPrice.GetTraderName(GetBestTrader().traderId);

		private int _avg24hPrice => CurrentItemScan.MatchedItems[0].GetAvg24hMarketPrice();

		private (string traderId, int price) GetBestTrader()
		{
			return MatchedItems[0].GetBestTrader();
		}

		private string PriceToString(int price)
		{
			if (CurrentItemScan.MatchedItems.Length == 1) return IntToGroupedString(price) + " ₽";

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

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged();
		}
	}
}
