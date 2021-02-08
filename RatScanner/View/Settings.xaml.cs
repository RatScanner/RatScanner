using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using RatScanner.ViewModel;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	internal partial class Settings : UserControl, ISwitchable
	{
		internal Settings()
		{
			InitializeComponent();
			DataContext = new SettingsVM();
			RatScannerMain.Instance.HotkeyManager.UnregisterHotkeys();
		}

		private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start("explorer.exe", e.Uri.ToString());
			e.Handled = true;
		}

		private void ClearIconCache(object sender, RoutedEventArgs e)
		{
			IconManager.ClearIconCache();
			((SettingsVM)DataContext).OnPropertyChanged();
		}

		private void CloseSettings(object sender, RoutedEventArgs e)
		{
			// Switch back to main menu
			PageSwitcher.Instance.Navigate(new MainMenu());
		}

		private void SaveSettings(object sender, RoutedEventArgs e)
		{
			Logger.LogInfo("Applying config...");

			var settingsVM = (SettingsVM)DataContext;

			// Pre saving stuff

			var updateMarketDB = settingsVM.NameScanLanguage != (int)RatConfig.NameScan.Language;

			// Save settings

			RatConfig.NameScan.Enable = settingsVM.EnableNameScan;
			RatConfig.NameScan.Language = (ApiManager.Language)settingsVM.NameScanLanguage;

			RatConfig.IconScan.Enable = settingsVM.EnableIconScan;
			RatConfig.IconScan.ScanRotatedIcons = settingsVM.ScanRotatedIcons;
			RatConfig.IconScan.UseCachedIcons = settingsVM.UseCachedIcons;
			RatConfig.IconScan.Hotkey = settingsVM.IconScanHotkey;

			RatConfig.ToolTip.Duration = int.TryParse(settingsVM.ToolTipDuration, out var i) ? i : 0;

			RatConfig.MinimalUi.ShowName = settingsVM.ShowName;
			RatConfig.MinimalUi.ShowPrice = settingsVM.ShowPrice;
			RatConfig.MinimalUi.ShowAvgDayPrice = settingsVM.ShowAvgDayPrice;
			RatConfig.MinimalUi.ShowAvgWeekPrice = settingsVM.ShowAvgWeekPrice;
			RatConfig.MinimalUi.ShowPricePerSlot = settingsVM.ShowPricePerSlot;
			RatConfig.MinimalUi.ShowTraderPrice = settingsVM.ShowTraderPrice;
			RatConfig.MinimalUi.ShowUpdated = settingsVM.ShowUpdated;
			RatConfig.MinimalUi.Opacity = settingsVM.Opacity;

			RatConfig.ScreenResolution = (RatConfig.Resolution)settingsVM.ScreenResolution;
			RatConfig.MinimizeToTray = settingsVM.MinimizeToTray;
			RatConfig.AlwaysOnTop = settingsVM.AlwaysOnTop;
			RatConfig.LogDebug = settingsVM.LogDebug;

			Logger.LogInfo("Saving config...");
			RatConfig.SaveConfig();

			// Apply config
			PageSwitcher.Instance.Topmost = RatConfig.AlwaysOnTop;
			if (updateMarketDB) RatScannerMain.Instance.MarketDB.Init();
			RatScannerMain.Instance.HotkeyManager.RegisterHotkeys();
			Logger.LogInfo("Config saved!");

			// Switch back to main menu
			PageSwitcher.Instance.Navigate(new MainMenu());
		}

		public void UtilizeState(object state)
		{
			throw new System.NotImplementedException();
		}

		public void OnClose()
		{
			RatScannerMain.Instance.HotkeyManager.RegisterHotkeys();
		}
	}
}
