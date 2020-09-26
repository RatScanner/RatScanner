using System.Windows;
using RatScanner.ViewModel;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	internal partial class Settings : Window
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
			Close();
		}

		private void SaveSettings(object sender, RoutedEventArgs e)
		{
			var settingsVM = (SettingsVM)DataContext;

			RatConfig.ToolTipDuration = int.TryParse(settingsVM.ToolTipDuration, out var i) ? i : 0;
			RatConfig.EnableNameScan = settingsVM.EnableNameScan;
			RatConfig.EnableIconScan = settingsVM.EnableIconScan;
			RatConfig.ScanRotatedIcons = settingsVM.ScanRotatedIcons;
			RatConfig.UseCachedIcons = settingsVM.UseCachedIcons;
			RatConfig.ModifierKeyCode = settingsVM.IconScanModifier;
			RatConfig.ScreenResolution = (RatConfig.Resolution)settingsVM.ScreenResolution;
			RatConfig.MinimizeToTray = settingsVM.MinimizeToTray;
			RatConfig.AlwaysOnTop = settingsVM.AlwaysOnTop;
			RatConfig.LogDebug = settingsVM.LogDebug;

			Logger.LogInfo("Saving config...");
			RatConfig.SaveConfig();
			Close();
		}
	}
}
