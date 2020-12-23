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
		public int returnLg;

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

			//Check Language
			switch (lgIndex)
			{
				default:
					lgIndex = 0;
					returnLg = 0;
				break;
				case 0:
					TranslateEN();
					returnLg = 0;
					break;
				case 3:
					TranslateFR();
					returnLg = 3;
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
			PageSwitcher.Instance.Navigate(new MainMenu(returnLg));
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
			PageSwitcher.Instance.Navigate(new MainMenu(returnLg));
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
			GeneralHeader.Header = "General";

			ToolTipDurationLabel.Content = "ToolTip Duration";
			ResolutionLabel.Content = "Resolution";
			MinimizeToTrayLabel.Content = "Minimize to Tray";
			AlwaysOnTopLabel.Content = "Always on Top";
			LogDebugInfoLabel.Content = "Log Debug Info";

			SaveButtonContent.Content = "Save";
			CancelButtonContent.Content = "Cancel";

			// Scanning
			ScanningHeader.Header = "Scanning";

			EnableNameScanLabel.Content = "Enable Name Scan";
			NameScanLanguageLabel.Content = "Name Scan Language";

			EnableIconScanLabel.Content = "Enable Icon Scan";
			ScanRotatedIconsLabel.Content = "Scan Rotated Icons";
			UseCachedIconsLabel.Content = "Use Cached Icons";
			IconCacheFolderLabel.Content = "Icon Cache Folder";
			ClearIconCacheButton.Content = "Clear";
			IconScanModifierLabel.Content = "Icon Scan Modifier";

			// Minimal UI
			MinimalUIHeader.Header = "Minimal UI";

			ShowNameLabel.Content = "Show Name";
			ShowPriceLabel.Content = "Show Price";
			ShowAvgDayPriceLabel.Content = "Show Average Day Price";
			ShowAvgWeekPriceLabel.Content = "Show Average Week Price";
			ShowPPSLabel.Content = "Show Price per Slot";
			ShowTraderPriceLabel.Content = "Show Trader Price";
			ShowUpdatedLabel.Content = "Show Updated";

			OpacityLabel.Content = "Opacity";

			// Info
			InfoHeader.Header = "Info";

			AppNameLabel.Content = "Rat Scanner";
			AttributionsLabel.Content = "Attributions";

			APIText.Text = "API for flea market price data";
			OpenCvText.Text = ".Net wrapper for OpenCV";
			JsonText.Text = "High-performance JSON framework for .NET";

			ThxText.Text = "Huge thanks to all Patrons who help me to make this project possible and keep it alive.";
		}

		private void TranslateFR()
		{
			// General
			GeneralHeader.Header = "Général";

			ToolTipDurationLabel.Content = "Durée du ToolTip";
			ResolutionLabel.Content = "Résolution";
			MinimizeToTrayLabel.Content = "Réduire dans le systray";
			AlwaysOnTopLabel.Content = "Toujours Visible";
			LogDebugInfoLabel.Content = "Garder les infos de debug";

			SaveButtonContent.Content = "Sauvegarder";
			CancelButtonContent.Content = "Annuler";

			// Scanning
			ScanningHeader.Header = "Scan";

			EnableNameScanLabel.Content = "Activer le scan du nom";
			NameScanLanguageLabel.Content = "Langue du scan du nom";

			EnableIconScanLabel.Content = "Activer le scan de l'icône";
			ScanRotatedIconsLabel.Content = "Scanner les icônes en rotation";
			UseCachedIconsLabel.Content = "Utiliser les icônes en cache";
			IconCacheFolderLabel.Content = "Fichier cache - Icônes";
			ClearIconCacheButton.Content = "Vider";
			IconScanModifierLabel.Content = "Touche de scan";

			// Minimal UI
			MinimalUIHeader.Header = "Mini-interface";

			ShowNameLabel.Content = "Montrer le nom";
			ShowPriceLabel.Content = "Montrer le prix";
			ShowAvgDayPriceLabel.Content = "Montrer le prix moyen - Journée";
			ShowAvgWeekPriceLabel.Content = "Montrer le prix moyen - Semaine";
			ShowPPSLabel.Content = "Montrer le prix par case";
			ShowTraderPriceLabel.Content = "Montrer le prix du marchand";
			ShowUpdatedLabel.Content = "Montrer la dernière MaJ";

			OpacityLabel.Content = "Opacité";

			// Info
			InfoHeader.Header = "Infos";

			AppNameLabel.Content = "Rat Scanner";
			AttributionsLabel.Content = "Attributions";

			APIText.Text = "API pour les prix du marché";
			OpenCvText.Text = "Wrapper .Net pour OpenCV";
			JsonText.Text = "JSON framework haute-performance pour .NET";

			ThxText.Text = "Un grand merci à tous les Patrons qui m'aident à faire que ce projet soit possible et le maintiennent en vie"; 


		}



	}
}
