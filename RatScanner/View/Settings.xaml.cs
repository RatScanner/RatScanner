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
		private readonly Task scanLockTask;

		internal Settings(int lgIndex)
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

			switch (lgIndex)
			{
				default:
					lgIndex = 0;
				break;
				case 0:
					TranslateEN();
					break;
				case 3:
					TranslateFR();
					break;

			}
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
			if (!scanLockTask.Wait(1000))
			{
				Logger.LogWarning("Could not save settings. Scan lock not acquired!");
				return;
			}

			var settingsVM = (SettingsVM)DataContext;

			// Pre saving stuff

			var updateMarketDB = settingsVM.NameScanLanguage != (int)RatConfig.NameScan.Language;

			// Save settings

			RatConfig.NameScan.Enable = settingsVM.EnableNameScan;
			RatConfig.NameScan.Language = (ApiManager.Language)settingsVM.NameScanLanguage;

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
			if (updateMarketDB) RatScannerMain.Instance.MarketDB.Init();

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

		private void TranslateEN()
		{
			// General
			GeneralHeader.Content = "General";

			ToolTipDurationLabel.Content = "ToolTip Duration";
			ResolutionLabel.Content = "Resolution";
			MinimizeToTrayLabel.Content = "Minimize to Tray";
			AlwaysOnTopLabel.Content = "Always on Top";
			LogDebugInfoLabel.Content = "Log Debug Info";

			// Scanning
			ScanningHeader.Content = "Scanning";

			EnableNameScanLabel.Content = "Enable Name Scan";
			NameScanLanguageLabel.Content = "Name Scan Language";

			EnableIconScanLabel.Content = "Enable Icon Scan";
			ScanRotatedIconsLabel.Content = "Scan Rotated Icons";
			UseCachedIconsLabel.Content = "Use Cached Icons";
			IconCacheFolderLabel.Content = "Icon Cache Folder";
			IconScanModifierLabel.Content = "Icon Scan Modifier";
		}

		private void TranslateFR()
		{
			// General
			GeneralHeader.Content = "Général";

			ToolTipDurationLabel.Content = "Durée du ToolTip";
			ResolutionLabel.Content = "Résolution";
			MinimizeToTrayLabel.Content = "Réduire dans le systray";
			AlwaysOnTopLabel.Content = "Toujours Visible";
			LogDebugInfoLabel.Content = "Garder les infos de debug";

			// Scanning
			ScanningHeader.Content = "Scan";

			EnableNameScanLabel.Content = "Activer le scan du nom";
			NameScanLanguageLabel.Content = "Langue du scan du nom";

			EnableIconScanLabel.Content = "Activer le scan de l'icône";
			ScanRotatedIconsLabel.Content = "Scanner les icônes en rotation";
			UseCachedIconsLabel.Content = "Utiliser les icônes en cache";
			IconCacheFolderLabel.Content = "Fichier cache des icônes";
			IconScanModifierLabel.Content = "Touche de scan : Clic Gauche + ";
		}



	}
}
