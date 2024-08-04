﻿using RatStash;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using RatScanner.TarkovTools;

namespace RatScanner.ViewModel;

internal class SettingsVM : INotifyPropertyChanged
{
	public bool EnableNameScan { get; set; }
	public bool EnableAutoNameScan { get; set; }
	public int NameScanLanguage { get; set; }

	public bool EnableIconScan { get; set; }
	public bool ScanRotatedIcons { get; set; }
	public bool UseCachedIcons { get; set; }
	public Hotkey IconScanHotkey { get; set; }

	public string ToolTipDuration { get; set; }
	public int ToolTipMilli { get; set; }

	public bool ShowName { get; set; }
	public bool ShowAvgDayPrice { get; set; }
	public bool ShowPricePerSlot { get; set; }
	public bool ShowTraderPrice { get; set; }
	public bool ShowUpdated { get; set; }
	public bool ShowQuestHideoutTracker { get; set; }
	public bool ShowQuestHideoutTeamTracker { get; set; }
	public int Opacity { get; set; }

	public int ScreenWidth { get; set; }
	public int ScreenHeight { get; set; }
	public float ScreenScale { get; set; }
	public bool MinimizeToTray { get; set; }
	public bool AlwaysOnTop { get; set; }
	public bool LogDebug { get; set; }

	// Progress Tracking Settings
	public bool ShowNonFIRNeeds { get; set; }

	// TarkovTracker Specific Tracking Settings
	public string TarkovTrackerToken { get; set; }

	public bool ShowTarkovTrackerTeam { get; set; }

	// Interactable Overlay
	public bool EnableIneractableOverlay { get; set; }
	public bool BlurBehindSearch { get; set; }
	public Hotkey InteractableOverlayHotkey { get; set; }

	// Tarkov Tools settings
	public string TarkovToolsRemoteControlSessionId { get; set; }
	public bool TarkovToolsRemoteControlAutoSync { get; set; }
	public bool TarkovToolsRemoteControlOpenAmmoChart { get; set; }

	internal SettingsVM()
	{
		LoadSettings();
	}

	public void LoadSettings()
	{
		EnableNameScan = RatConfig.NameScan.Enable;
		EnableAutoNameScan = RatConfig.NameScan.EnableAuto;
		NameScanLanguage = (int)RatConfig.NameScan.Language;

		EnableIconScan = RatConfig.IconScan.Enable;
		ScanRotatedIcons = RatConfig.IconScan.ScanRotatedIcons;
		UseCachedIcons = RatConfig.IconScan.UseCachedIcons;
		IconScanHotkey = RatConfig.IconScan.Hotkey;

		ToolTipDuration = RatConfig.ToolTip.Duration.ToString();
		ToolTipMilli = RatConfig.ToolTip.Duration;

		ShowName = RatConfig.MinimalUi.ShowName;
		ShowAvgDayPrice = RatConfig.MinimalUi.ShowAvgDayPrice;
		ShowPricePerSlot = RatConfig.MinimalUi.ShowPricePerSlot;
		ShowTraderPrice = RatConfig.MinimalUi.ShowTraderPrice;
		ShowQuestHideoutTracker = RatConfig.MinimalUi.ShowQuestHideoutTracker;
		ShowQuestHideoutTeamTracker = RatConfig.MinimalUi.ShowQuestHideoutTeamTracker;
		ShowUpdated = RatConfig.MinimalUi.ShowUpdated;
		Opacity = RatConfig.MinimalUi.Opacity;

		ScreenWidth = RatConfig.ScreenWidth;
		ScreenHeight = RatConfig.ScreenHeight;
		ScreenScale = RatConfig.ScreenScale;
		MinimizeToTray = RatConfig.MinimizeToTray;
		AlwaysOnTop = RatConfig.AlwaysOnTop;
		LogDebug = RatConfig.LogDebug;

		ShowNonFIRNeeds = RatConfig.Tracking.ShowNonFIRNeeds;

		TarkovTrackerToken = RatConfig.Tracking.TarkovTracker.Token;
		ShowTarkovTrackerTeam = RatConfig.Tracking.TarkovTracker.ShowTeam;

		EnableIneractableOverlay = RatConfig.Overlay.Search.Enable;
		BlurBehindSearch = RatConfig.Overlay.Search.BlurBehind;
		InteractableOverlayHotkey = RatConfig.Overlay.Search.Hotkey;

		TarkovToolsRemoteControlSessionId = RatConfig.TarkovTools.RemoteControl.SessionId;
		TarkovToolsRemoteControlAutoSync = RatConfig.TarkovTools.RemoteControl.AutoSync;
		TarkovToolsRemoteControlOpenAmmoChart = RatConfig.TarkovTools.RemoteControl.OpenAmmoChart;

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
	}

	public void SaveSettings()
	{
		var updateMarketDB = NameScanLanguage != (int)RatConfig.NameScan.Language;
		var updateTarkovTrackerToken = TarkovTrackerToken != RatConfig.Tracking.TarkovTracker.Token;
		var updateResolution = ScreenWidth != RatConfig.ScreenWidth || ScreenHeight != RatConfig.ScreenHeight;
		var updateLanguage = RatConfig.NameScan.Language != (Language)NameScanLanguage;
		var updateSessionId = RatConfig.TarkovTools.RemoteControl.SessionId != TarkovToolsRemoteControlSessionId;

		// Save config
		RatConfig.NameScan.Enable = EnableNameScan;
		RatConfig.NameScan.EnableAuto = EnableAutoNameScan;
		RatConfig.NameScan.Language = (Language)NameScanLanguage;

		RatConfig.IconScan.Enable = EnableIconScan;
		RatConfig.IconScan.ScanRotatedIcons = ScanRotatedIcons;
		RatConfig.IconScan.UseCachedIcons = UseCachedIcons;
		RatConfig.IconScan.Hotkey = IconScanHotkey;

		RatConfig.ToolTip.Duration = int.TryParse(ToolTipDuration, out var i) ? i : 0;
		RatConfig.ToolTip.Duration = ToolTipMilli;

		RatConfig.MinimalUi.ShowName = ShowName;
		RatConfig.MinimalUi.ShowAvgDayPrice = ShowAvgDayPrice;
		RatConfig.MinimalUi.ShowPricePerSlot = ShowPricePerSlot;
		RatConfig.MinimalUi.ShowTraderPrice = ShowTraderPrice;
		RatConfig.MinimalUi.ShowQuestHideoutTracker = ShowQuestHideoutTracker;
		RatConfig.MinimalUi.ShowQuestHideoutTeamTracker = ShowQuestHideoutTeamTracker;
		RatConfig.MinimalUi.ShowUpdated = ShowUpdated;
		RatConfig.MinimalUi.Opacity = Opacity;

		RatConfig.Tracking.ShowNonFIRNeeds = ShowNonFIRNeeds;

		RatConfig.Tracking.TarkovTracker.Token = TarkovTrackerToken.Trim();
		RatConfig.Tracking.TarkovTracker.ShowTeam = ShowTarkovTrackerTeam;

		RatConfig.Overlay.Search.Enable = EnableIneractableOverlay;
		RatConfig.Overlay.Search.BlurBehind = BlurBehindSearch;
		RatConfig.Overlay.Search.Hotkey = InteractableOverlayHotkey;

		RatConfig.TarkovTools.RemoteControl.SessionId = TarkovToolsRemoteControlSessionId;
		RatConfig.TarkovTools.RemoteControl.AutoSync = TarkovToolsRemoteControlAutoSync;
		RatConfig.TarkovTools.RemoteControl.OpenAmmoChart = TarkovToolsRemoteControlOpenAmmoChart;

		RatConfig.ScreenWidth = ScreenWidth;
		RatConfig.ScreenHeight = ScreenHeight;
		RatConfig.ScreenScale = ScreenScale;
		RatConfig.MinimizeToTray = MinimizeToTray;
		RatConfig.AlwaysOnTop = AlwaysOnTop;
		RatConfig.LogDebug = LogDebug;

		// Apply config
		PageSwitcher.Instance.Topmost = RatConfig.AlwaysOnTop;
		//if (updateMarketDB) RatScannerMain.Instance.MarketDB.Init();//TODO
		if (updateTarkovTrackerToken) UpdateTarkovTrackerToken();
		if (updateResolution || updateLanguage) RatScannerMain.Instance.SetupRatEye();
		if (updateSessionId) UpdateTarkovToolsRemoteController();

		RatEye.Config.LogDebug = RatConfig.LogDebug;
		RatScannerMain.Instance.HotkeyManager.RegisterHotkeys();

		// Save config to file
		Logger.LogInfo("Saving config...");
		RatConfig.SaveConfig();
		Logger.LogInfo("Config saved!");
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
	}

	private void UpdateTarkovTrackerToken()
	{
		var token = RatConfig.Tracking.TarkovTracker.Token;
		if (token == "") return;
		RatScannerMain.Instance.TarkovTrackerDB.Token = RatConfig.Tracking.TarkovTracker.Token;
		if (RatScannerMain.Instance.TarkovTrackerDB.Init()) return;

		var visibleLength = (int)(token.Length * 0.25);
		token = token[..visibleLength] + string.Concat(Enumerable.Repeat(" *", token.Length - visibleLength));
		Logger.ShowWarning($"The TarkovTracker API Token does not seem to work.\n\n{token}");

		RatConfig.Tracking.TarkovTracker.Token = "";
	}

	private static void UpdateTarkovToolsRemoteController()
	{
		var controller = RatScannerMain.Instance.TarkovToolsRemoteController;
		if (RatConfig.TarkovTools.RemoteControl.Enable)
		{
			try
			{
				controller.Connect(RatConfig.TarkovTools.RemoteControl.SessionId);
			}
			catch (TarkovToolsRemoteControllerException e)
			{
				Logger.LogWarning(e.Message, e);
			}
		}
		else
		{
			 controller.Disconnect();
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	internal virtual void OnPropertyChanged(string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
