using RatStash;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RatScanner.ViewModel;

internal class SettingsVM : INotifyPropertyChanged {
	public bool EnableNameScan { get; set; }
	public bool EnableAutoNameScan { get; set; }
	public int NameScanLanguage { get; set; }

	public bool EnableIconScan { get; set; }
	public bool ScanRotatedIcons { get; set; }
	public bool UseCachedIcons { get; set; }
	public Hotkey IconScanHotkey { get; set; }

	public string ToolTipDuration { get; set; }
	public int ToolTipMilli { get; set; }
	public UiLanguage UiLanguage { get; set; }

	public bool ShowName { get; set; }
	public bool ShowAvgDayPrice { get; set; }
	public bool ShowPricePerSlot { get; set; }
	public bool ShowTraderPrice { get; set; }
	public bool ShowUpdated { get; set; }
	public bool ShowKappa { get; set; }
	public bool ShowQuestHideoutTracker { get; set; }
	public bool ShowQuestHideoutTeamTracker { get; set; }
	public int Opacity { get; set; }

	public int ScreenWidth { get; set; }
	public int ScreenHeight { get; set; }
	public float ScreenScale { get; set; }
	public GameMode GameMode { get; set; }
	public bool MinimizeToTray { get; set; }
	public bool AlwaysOnTop { get; set; }
	public bool LogDebug { get; set; }

	// Progress Tracking Settings
	public bool ShowNonFIRNeeds { get; set; }

	public bool ShowKappaNeeds { get; set; }

	// TarkovTracker Specific Tracking Settings
	private readonly Dictionary<GameMode, string> _tarkovTrackerTokens = new();

	/// <summary>Token belonging to the currently selected <see cref="GameMode"/></summary>
	public string TarkovTrackerToken {
		get => _tarkovTrackerTokens.TryGetValue(GameMode, out string? token) ? token : "";
		set => _tarkovTrackerTokens[GameMode] = value;
	}

	public bool ShowTarkovTrackerTeam { get; set; }

	public RatConfig.TarkovTrackerBackend TarkovTrackerBackend { get; set; }

	// Interactable Overlay
	public bool EnableIneractableOverlay { get; set; }
	public bool BlurBehindSearch { get; set; }
	public Hotkey InteractableOverlayHotkey { get; set; }

	private readonly LocalizationService _localizationService;

	internal SettingsVM(LocalizationService localizationService) {
		_localizationService = localizationService;
		LoadSettings();
	}

	public void LoadSettings() {
		EnableNameScan = RatConfig.NameScan.Enable;
		EnableAutoNameScan = RatConfig.NameScan.EnableAuto;
		NameScanLanguage = (int)RatConfig.NameScan.Language;

		EnableIconScan = RatConfig.IconScan.Enable;
		ScanRotatedIcons = RatConfig.IconScan.ScanRotatedIcons;
		UseCachedIcons = RatConfig.IconScan.UseCachedIcons;
		IconScanHotkey = RatConfig.IconScan.Hotkey;

		ToolTipDuration = RatConfig.ToolTip.Duration.ToString();
		ToolTipMilli = RatConfig.ToolTip.Duration;
		UiLanguage = RatConfig.UserInterface.Language;

		ShowName = RatConfig.MinimalUi.ShowName;
		ShowAvgDayPrice = RatConfig.MinimalUi.ShowAvgDayPrice;
		ShowPricePerSlot = RatConfig.MinimalUi.ShowPricePerSlot;
		ShowTraderPrice = RatConfig.MinimalUi.ShowTraderPrice;
		ShowKappa = RatConfig.MinimalUi.ShowKappa;
		ShowQuestHideoutTracker = RatConfig.MinimalUi.ShowQuestHideoutTracker;
		ShowQuestHideoutTeamTracker = RatConfig.MinimalUi.ShowQuestHideoutTeamTracker;
		ShowUpdated = RatConfig.MinimalUi.ShowUpdated;
		Opacity = RatConfig.MinimalUi.Opacity;

		ScreenWidth = RatConfig.ScreenWidth;
		ScreenHeight = RatConfig.ScreenHeight;
		ScreenScale = RatConfig.ScreenScale;
		GameMode = RatConfig.GameMode;
		MinimizeToTray = RatConfig.MinimizeToTray;
		AlwaysOnTop = RatConfig.AlwaysOnTop;
		LogDebug = RatConfig.LogDebug;

		ShowNonFIRNeeds = RatConfig.Tracking.ShowNonFIRNeeds;
		ShowKappaNeeds = RatConfig.Tracking.ShowKappaNeeds;

		foreach (GameMode gameMode in Enum.GetValues<GameMode>()) {
			_tarkovTrackerTokens[gameMode] = RatConfig.Tracking.TarkovTracker.GetToken(gameMode);
		}
		ShowTarkovTrackerTeam = RatConfig.Tracking.TarkovTracker.ShowTeam;
		TarkovTrackerBackend = RatConfig.Tracking.TarkovTracker.Backend;

		EnableIneractableOverlay = RatConfig.Overlay.Search.Enable;
		BlurBehindSearch = RatConfig.Overlay.Search.BlurBehind;
		InteractableOverlayHotkey = RatConfig.Overlay.Search.Hotkey;

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
	}

	/// <returns>False if the settings were saved but the TarkovTracker token was rejected</returns>
	public async Task<bool> SaveSettings() {
		bool updateMarketDB = NameScanLanguage != (int)RatConfig.NameScan.Language;
		// The active token follows the game mode, so switching mode swaps tokens too
		bool updateTarkovTrackerToken = GameMode != RatConfig.GameMode
			|| _tarkovTrackerTokens.Any(token => token.Value.Trim() != RatConfig.Tracking.TarkovTracker.GetToken(token.Key));
		bool updateTarkovTrackerBackend = TarkovTrackerBackend != RatConfig.Tracking.TarkovTracker.Backend;
		bool updateResolution = ScreenWidth != RatConfig.ScreenWidth || ScreenHeight != RatConfig.ScreenHeight;
		bool updateLanguage = RatConfig.NameScan.Language != (Language)NameScanLanguage;
		bool updateUiLanguage = RatConfig.UserInterface.Language != UiLanguage;

		// Save config
		RatConfig.NameScan.Enable = EnableNameScan;
		RatConfig.NameScan.EnableAuto = EnableAutoNameScan;
		RatConfig.NameScan.Language = (Language)NameScanLanguage;

		RatConfig.IconScan.Enable = EnableIconScan;
		RatConfig.IconScan.ScanRotatedIcons = ScanRotatedIcons;
		RatConfig.IconScan.UseCachedIcons = UseCachedIcons;
		RatConfig.IconScan.Hotkey = IconScanHotkey;

		RatConfig.ToolTip.Duration = int.TryParse(ToolTipDuration, out int i) ? i : 0;
		RatConfig.ToolTip.Duration = ToolTipMilli;
		RatConfig.UserInterface.Language = UiLanguage;

		RatConfig.MinimalUi.ShowName = ShowName;
		RatConfig.MinimalUi.ShowAvgDayPrice = ShowAvgDayPrice;
		RatConfig.MinimalUi.ShowPricePerSlot = ShowPricePerSlot;
		RatConfig.MinimalUi.ShowTraderPrice = ShowTraderPrice;
		RatConfig.MinimalUi.ShowKappa = ShowKappa;
		RatConfig.MinimalUi.ShowQuestHideoutTracker = ShowQuestHideoutTracker;
		RatConfig.MinimalUi.ShowQuestHideoutTeamTracker = ShowQuestHideoutTeamTracker;
		RatConfig.MinimalUi.ShowUpdated = ShowUpdated;
		RatConfig.MinimalUi.Opacity = Opacity;

		RatConfig.Tracking.ShowNonFIRNeeds = ShowNonFIRNeeds;
		RatConfig.Tracking.ShowKappaNeeds = ShowKappaNeeds;

		// Written per mode instead of through the mode aware Token property, which
		// would otherwise depend on RatConfig.GameMode being updated first
		foreach (KeyValuePair<GameMode, string> token in _tarkovTrackerTokens) {
			RatConfig.Tracking.TarkovTracker.SetToken(token.Key, token.Value.Trim());
		}
		RatConfig.Tracking.TarkovTracker.ShowTeam = ShowTarkovTrackerTeam;
		RatConfig.Tracking.TarkovTracker.Backend = TarkovTrackerBackend;

		RatConfig.Overlay.Search.Enable = EnableIneractableOverlay;
		RatConfig.Overlay.Search.BlurBehind = BlurBehindSearch;
		RatConfig.Overlay.Search.Hotkey = InteractableOverlayHotkey;

		RatConfig.ScreenWidth = ScreenWidth;
		RatConfig.ScreenHeight = ScreenHeight;
		RatConfig.ScreenScale = ScreenScale;
		RatConfig.GameMode = GameMode;
		RatConfig.MinimizeToTray = MinimizeToTray;
		RatConfig.AlwaysOnTop = AlwaysOnTop;
		RatConfig.LogDebug = LogDebug;

		// Apply config
		PageSwitcher.Instance.Topmost = RatConfig.AlwaysOnTop;
		PageSwitcher.Instance.ResetWindowSize();
		await TarkovDevAPI.InitializeCache();
		bool tokenAccepted = true;
		if (updateTarkovTrackerToken || updateTarkovTrackerBackend) tokenAccepted = UpdateTarkovTrackerToken();
		if (updateUiLanguage) _localizationService.SetLanguage(UiLanguage);
		if (updateResolution || updateLanguage) RatScannerMain.Instance.SetupRatEye();

		RatEye.Config.LogDebug = RatConfig.LogDebug;
		RatScannerMain.Instance.HotkeyManager.RegisterHotkeys();

		// Save config to file
		Logger.LogInfo("Saving config...");
		RatConfig.SaveConfig();
		Logger.LogInfo("Config saved!");
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
		return tokenAccepted;
	}

	/// <returns>False if the token was rejected and therefore removed</returns>
	private bool UpdateTarkovTrackerToken() {
		string token = RatConfig.Tracking.TarkovTracker.Token;
		if (token == "") return true;
		RatScannerMain.Instance.TarkovTrackerDB.Token = RatConfig.Tracking.TarkovTracker.Token;
		var db = RatScannerMain.Instance.TarkovTrackerDB;
		if (db.TestToken(token)) {
			db.UpdateToken();
			return true;
		}

		int visibleLength = (int)(token.Length * 0.25);
		token = token[..visibleLength] + string.Concat(Enumerable.Repeat(" *", token.Length - visibleLength));
		Logger.ShowWarning($"The TarkovTracker API Token does not seem to work.\n\n{token}");

		RatConfig.Tracking.TarkovTracker.Token = "";
		return false;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	internal virtual void OnPropertyChanged(string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
