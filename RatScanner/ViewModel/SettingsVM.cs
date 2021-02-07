using System.ComponentModel;
using RatScanner.Controls;

namespace RatScanner.ViewModel
{
	internal class SettingsVM : INotifyPropertyChanged
	{
		public bool EnableNameScan { get; set; }
		public int NameScanLanguage { get; set; }

		public bool EnableIconScan { get; set; }
		public bool ScanRotatedIcons { get; set; }
		public bool UseCachedIcons { get; set; }
		public int IconCacheSize => IconManager.GetIconCacheSize();
		public Hotkey IconScanHotkey { get; set; }

		public string ToolTipDuration { get; set; }

		public bool ShowName { get; set; }
		public bool ShowPrice { get; set; }
		public bool ShowAvgDayPrice { get; set; }
		public bool ShowAvgWeekPrice { get; set; }
		public bool ShowPricePerSlot { get; set; }
		public bool ShowTraderPrice { get; set; }
		public bool ShowUpdated { get; set; }
		public int Opacity { get; set; }

		public int ScreenResolution { get; set; }
		public bool MinimizeToTray { get; set; }
		public bool AlwaysOnTop { get; set; }
		public bool LogDebug { get; set; }

		internal SettingsVM()
		{
			EnableNameScan = RatConfig.NameScan.Enable;
			NameScanLanguage = (int)RatConfig.NameScan.Language;

			EnableIconScan = RatConfig.IconScan.Enable;
			ScanRotatedIcons = RatConfig.IconScan.ScanRotatedIcons;
			UseCachedIcons = RatConfig.IconScan.UseCachedIcons;
			IconScanHotkey = RatConfig.IconScan.Hotkey;

			ToolTipDuration = RatConfig.ToolTip.Duration.ToString();

			ShowName = RatConfig.MinimalUi.ShowName;
			ShowPrice = RatConfig.MinimalUi.ShowPrice;
			ShowAvgDayPrice = RatConfig.MinimalUi.ShowAvgDayPrice;
			ShowAvgWeekPrice = RatConfig.MinimalUi.ShowAvgWeekPrice;
			ShowPricePerSlot = RatConfig.MinimalUi.ShowPricePerSlot;
			ShowTraderPrice = RatConfig.MinimalUi.ShowTraderPrice;
			ShowUpdated = RatConfig.MinimalUi.ShowUpdated;
			Opacity = RatConfig.MinimalUi.Opacity;

			ScreenResolution = (int)RatConfig.ScreenResolution;
			MinimizeToTray = RatConfig.MinimizeToTray;
			AlwaysOnTop = RatConfig.AlwaysOnTop;
			LogDebug = RatConfig.LogDebug;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		internal virtual void OnPropertyChanged(string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
