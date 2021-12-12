using RatLib.Scan;
using RatStash;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
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

		private Item[] MatchedItems => DataSource.MatchedItems;

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable InconsistentNaming
		public string Avg24hPrice => PriceToString(GetAvg24hPrice());

		private int GetAvg24hPrice()
		{
			return MatchedItems[0].GetAvg24hMarketPrice();
		}
		// ReSharper restore InconsistentNaming

		public string IconPath => DataSource.IconPath ?? RatConfig.Paths.UnknownIcon;

		public int IconAngle => DataSource is ItemIconScan scan && scan.Rotated ? 90 : 0;

		public static float ScaleFactor => RatConfig.ScreenScale / GetScalingFactor();

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
			if (MatchedItems.Length == 1) return IntToGroupedString(price) + " ₽";

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

		/// <summary>
		/// Compute the scaling factor used for the primary screen
		/// </summary>
		/// <returns>How many physical pixels are used for a virtual pixel</returns>
		private static float GetScalingFactor()
		{
			var physicalHeight = Screen.PrimaryScreen.Bounds.Height; // Virtual Height
			var virtualHeight = SystemParameters.PrimaryScreenHeight; // Physical Height
			var scaleFactor = physicalHeight / virtualHeight;
			return (float)scaleFactor;
		}
	}
}
