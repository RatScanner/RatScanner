using System.Diagnostics;
using System.Linq;
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
			PageSwitcher.Instance.ResetWindowSize();
			RatScannerMain.Instance.HotkeyManager.UnregisterHotkeys();
		}

		private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start("explorer.exe", e.Uri.ToString());
			e.Handled = true;
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
			var updateTarkovTrackerToken = settingsVM.TarkovTrackerToken != RatConfig.Tracking.TarkovTracker.Token;

			// Save config
			RatConfig.NameScan.Enable = settingsVM.EnableNameScan;
			RatConfig.NameScan.Language = (ApiManager.Language)settingsVM.NameScanLanguage;

			RatConfig.IconScan.Enable = settingsVM.EnableIconScan;
			RatConfig.IconScan.ScanRotatedIcons = settingsVM.ScanRotatedIcons;
			RatConfig.IconScan.UseCachedIcons = settingsVM.UseCachedIcons;
			RatConfig.IconScan.Hotkey = settingsVM.IconScanHotkey;

			RatConfig.ToolTip.Duration = int.TryParse(settingsVM.ToolTipDuration, out var i) ? i : 0;

			RatConfig.MinimalUi.ShowName = settingsVM.ShowName;
			RatConfig.MinimalUi.ShowAvgDayPrice = settingsVM.ShowAvgDayPrice;
			RatConfig.MinimalUi.ShowPricePerSlot = settingsVM.ShowPricePerSlot;
			RatConfig.MinimalUi.ShowTraderPrice = settingsVM.ShowTraderPrice;
			RatConfig.MinimalUi.ShowTraderMaxPrice = settingsVM.ShowTraderMaxPrice;
			RatConfig.MinimalUi.ShowQuestHideoutTracker = settingsVM.ShowQuestHideoutTracker;
			RatConfig.MinimalUi.ShowQuestHideoutTeamTracker = settingsVM.ShowQuestHideoutTeamTracker;
			RatConfig.MinimalUi.ShowUpdated = settingsVM.ShowUpdated;
			RatConfig.MinimalUi.Opacity = settingsVM.Opacity;

			RatConfig.Tracking.ShowNonFIRNeeds = settingsVM.ShowNonFIRNeeds;

			RatConfig.Tracking.TarkovTracker.Token = settingsVM.TarkovTrackerToken.Trim();
			RatConfig.Tracking.TarkovTracker.ShowTeam = settingsVM.ShowTarkovTrackerTeam;

			RatConfig.ScreenWidth = settingsVM.ScreenWidth;
			RatConfig.ScreenHeight = settingsVM.ScreenHeight;
			RatConfig.MinimizeToTray = settingsVM.MinimizeToTray;
			RatConfig.AlwaysOnTop = settingsVM.AlwaysOnTop;
			RatConfig.LogDebug = settingsVM.LogDebug;

			// Apply config
			PageSwitcher.Instance.Topmost = RatConfig.AlwaysOnTop;
			if (updateMarketDB) RatScannerMain.Instance.MarketDB.Init();
			if (updateTarkovTrackerToken) UpdateTarkovTrackerToken();
			var processingConfig = RatEye.Config.GlobalConfig.ProcessingConfig;
			RatEye.Config.GlobalConfig.Apply();
				processingConfig.Scale = Config.Processing.Resolution2Scale(RatConfig.ScreenWidth, RatConfig.ScreenHeight);
			RatScannerMain.Instance.HotkeyManager.RegisterHotkeys();

			// Save config to file
			Logger.LogInfo("Saving config...");
			RatConfig.SaveConfig();
			Logger.LogInfo("Config saved!");

			// Switch back to main menu
			PageSwitcher.Instance.Navigate(new MainMenu());
		}

		private void UpdateTarkovTrackerToken()
		{
			var token = RatConfig.Tracking.TarkovTracker.Token;
			if (token == "") return;
			if (RatScannerMain.Instance.TarkovTrackerDB.Init()) return;

			var visibleLength = (int)(token.Length * 0.25);
			token = token[..visibleLength] + string.Concat(Enumerable.Repeat(" *", token.Length - visibleLength));
			Logger.ShowWarning($"The TarkovTracker API Token does not seem to work.\n\n{token}");

			RatConfig.Tracking.TarkovTracker.Token = "";
		}

		public void UtilizeState(object state)
		{
			throw new System.NotImplementedException();
		}

		public void OnOpen() { }

		public void OnClose()
		{
			RatScannerMain.Instance.HotkeyManager.RegisterHotkeys();
		}
	}
}
