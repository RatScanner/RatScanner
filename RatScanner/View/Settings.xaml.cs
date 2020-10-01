using System.Windows;
using System.Windows.Controls;
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
		}

		private void ClearIconCache(object sender, RoutedEventArgs e)
		{
			IconManager.ClearIconCache();
			((SettingsVM)DataContext).OnPropertyChanged();
		}

		private void CloseSettings(object sender, RoutedEventArgs e)
		{
			PageSwitcher.Instance.Navigate(new MainMenu());
		}

		private void SaveSettings(object sender, RoutedEventArgs e)
		{
			var settingsVM = (SettingsVM)DataContext;

			RatConfig.NameScan.Enable = settingsVM.EnableNameScan;

			RatConfig.IconScan.Enable = settingsVM.EnableIconScan;
			RatConfig.IconScan.ScanRotatedIcons = settingsVM.ScanRotatedIcons;
			RatConfig.IconScan.UseCachedIcons = settingsVM.UseCachedIcons;
			RatConfig.IconScan.ModifierKeyCode = settingsVM.IconScanModifier;

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

			PageSwitcher.Instance.Navigate(new MainMenu());
		}

		public void UtilizeState(object state)
		{
			throw new System.NotImplementedException();
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			throw new System.NotImplementedException();
		}
	}
}
