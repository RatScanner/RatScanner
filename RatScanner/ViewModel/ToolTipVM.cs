using System.ComponentModel;
using System.Globalization;
using System.Windows.Media;
using RatScanner.FetchModels;
using RatScanner.Scan;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace RatScanner.ViewModel
{
	internal class ToolTipVM : INotifyPropertyChanged
	{
		private ItemScan _dataSource;

		internal ItemScan DataSource
		{
			get => _dataSource;
			set
			{
				_dataSource = value;
				OnPropertyChanged();
			}
		}

		private MarketItem[] MatchedItems => DataSource.MatchedItems;

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable InconsistentNaming
		public string Avg24hPrice => PriceToString(GetAvg24hPrice());

		private int GetAvg24hPrice()
		{
			return MatchedItems[0].SumMods(item => item.Avg24hPrice);
		}
		// ReSharper restore InconsistentNaming

		public string IconPath => IconManager.GetIconPath(DataSource.MatchedItems[0]);

		public int IconAngle => DataSource is ItemIconScan scan && scan.Rotated ? 90 : 0;

		public float ScaleFactor => RatConfig.GetScreenScaleFactor();

		public Brush WarningBrush
		{
			get
			{
				var color = Color.FromRgb(238, 238, 238);

				var threshold = 1.0f;
				if (DataSource is ItemNameScan) threshold = RatConfig.NameScan.ConfWarnThreshold;
				if (DataSource is ItemIconScan) threshold = RatConfig.IconScan.ConfWarnThreshold;

				if (DataSource.Confidence < threshold) color = Color.FromRgb(227, 38, 25);
				return new SolidColorBrush(color);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		internal ToolTipVM(ItemScan itemScan)
		{
			DataSource = itemScan;
		}

		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
