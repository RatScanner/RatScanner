using System.Threading;
using System.Threading.Tasks;
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
		private readonly Task scanLockTask;

		internal Settings()
		{
			InitializeComponent();
			DataContext = new SettingsVM();

			// Acquire scan lock to prevent issues when changing hot settings
			scanLockTask = Task.Run(() =>
			{
				// Wait if currently scanning a item
				while (RatScannerMain.Instance.ScanLock) Thread.Sleep(25);
				RatScannerMain.Instance.ScanLock = true;
			});
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
			if (!scanLockTask.Wait(1000))
			{
				Logger.LogWarning("Could not save settings. Scan lock not acquired!");
				return;
			}

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

			// Switch back to main menu
			PageSwitcher.Instance.Navigate(new MainMenu());
		}

		public void UtilizeState(object state)
		{
			throw new System.NotImplementedException();
		}

		public void OnClose()
		{
			if (!scanLockTask.Wait(1000))
			{
				scanLockTask.Dispose();
			}

			// Release scan lock
			RatScannerMain.Instance.ScanLock = false;
		}
	}
}
