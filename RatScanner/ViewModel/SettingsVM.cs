using RatScanner.TarkovDev.Json;
using RatStash;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RatScanner.ViewModel;

internal class SettingsVM : INotifyPropertyChanged {
	public bool EnableNameScan { get; set; }
	public bool EnableAutoNameScan { get; set; }
	public int NameScanLanguage { get; set; }
	public Hotkey NameScanHotkey { get; set; }

	public bool EnableIconScan { get; set; }
	public bool ScanRotatedIcons { get; set; }
	public bool UseCachedIcons { get; set; }
	public Hotkey IconScanHotkey { get; set; }

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
	public bool OverrideScreenConfig { get; set; }
	public GameMode GameMode { get; set; }
	public bool MinimizeToTray { get; set; }
	public bool AlwaysOnTop { get; set; }
	public bool LogDebug { get; set; }

	// Progress Tracking Settings
	public bool ShowNonFIRNeeds { get; set; }

	public bool ShowKappaNeeds { get; set; }

	// TarkovTracker Specific Tracking Settings
	public string TarkovTrackerToken { get; set; }

	public bool ShowTarkovTrackerTeam { get; set; }

	public RatConfig.TarkovTrackerBackend TarkovTrackerBackend { get; set; }

	// Interactable Overlay
	public bool EnableIneractableOverlay { get; set; }
	public bool BlurBehindSearch { get; set; }
	public Hotkey InteractableOverlayHotkey { get; set; }
	public Hotkey CloseOverlayHotkey { get; set; }

	// Application hotkeys
	public Hotkey OpenWikiHotkey { get; set; }
	public Hotkey OpenTarkovDevHotkey { get; set; }

	private readonly LocalizationService _localizationService;

	internal SettingsVM(LocalizationService localizationService) {
		_localizationService = localizationService;
		LoadSettings();
	}

	public void LoadSettings() {
		EnableNameScan = RatConfig.NameScan.Enable;
		EnableAutoNameScan = RatConfig.NameScan.EnableAuto;
		NameScanLanguage = (int)RatConfig.NameScan.Language;
		NameScanHotkey = new Hotkey(RatConfig.NameScan.Hotkey);

		EnableIconScan = RatConfig.IconScan.Enable;
		ScanRotatedIcons = RatConfig.IconScan.ScanRotatedIcons;
		UseCachedIcons = RatConfig.IconScan.UseCachedIcons;
		IconScanHotkey = new Hotkey(RatConfig.IconScan.Hotkey);

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
		OverrideScreenConfig = RatConfig.OverrideScreenConfig;
		GameMode = RatConfig.GameMode;
		MinimizeToTray = RatConfig.MinimizeToTray;
		AlwaysOnTop = RatConfig.AlwaysOnTop;
		LogDebug = RatConfig.LogDebug;

		ShowNonFIRNeeds = RatConfig.Tracking.ShowNonFIRNeeds;
		ShowKappaNeeds = RatConfig.Tracking.ShowKappaNeeds;

		TarkovTrackerToken = RatConfig.Tracking.TarkovTracker.Token;
		ShowTarkovTrackerTeam = RatConfig.Tracking.TarkovTracker.ShowTeam;
		TarkovTrackerBackend = RatConfig.Tracking.TarkovTracker.Backend;

		EnableIneractableOverlay = RatConfig.Overlay.Search.Enable;
		BlurBehindSearch = RatConfig.Overlay.Search.BlurBehind;
		InteractableOverlayHotkey = new Hotkey(RatConfig.Overlay.Search.Hotkey);
		CloseOverlayHotkey = new Hotkey(RatConfig.Overlay.Search.CloseHotkey);

		OpenWikiHotkey = new Hotkey(RatConfig.Hotkeys.OpenWiki);
		OpenTarkovDevHotkey = new Hotkey(RatConfig.Hotkeys.OpenTarkovDev);

		IsDirty = false;
		_suppressDirty = true;
		Notify();
	}

	/// <summary>
	/// True when the in-memory settings differ from the persisted config, so the
	/// UI can show a dirty state and enable the Save / Discard buttons.
	/// </summary>
	public bool IsDirty { get; private set; }

	private bool _suppressDirty;

	/// <summary>
	/// Clears the load-time suppression. Called once the re-render triggered by a
	/// reset has been processed, so a genuine edit after a load is never ignored.
	/// </summary>
	internal void EndLoadSuppression() {
		_suppressDirty = false;
	}

	private void Notify() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

	/// <summary>
	/// Marks the settings as having unsaved changes. Called by the settings pages
	/// whenever a control is edited.
	/// </summary>
	/// <remarks>
	/// The notification is raised on every call, not just the first, because pages
	/// use it as their signal to re-render (live previews, dependent rows, the
	/// footer status) whenever a control changes.
	/// </remarks>
	public void MarkDirty() {
		// Swallow the first callback after a load: that is a control re-applying
		// the value we just handed it, not a user edit.
		if (_suppressDirty) {
			_suppressDirty = false;
			return;
		}

		IsDirty = true;
		Notify();
	}

	public async Task SaveSettings() {
		_ = NameScanLanguage != (int)RatConfig.NameScan.Language;
		var updateTarkovTrackerToken = TarkovTrackerToken != RatConfig.Tracking.TarkovTracker.Token;
		var updateTarkovTrackerBackend = TarkovTrackerBackend != RatConfig.Tracking.TarkovTracker.Backend;
		var updateResolution = ScreenWidth != RatConfig.ScreenWidth || ScreenHeight != RatConfig.ScreenHeight;
		var updateScreenOverride = OverrideScreenConfig != RatConfig.OverrideScreenConfig || ScreenScale != RatConfig.ScreenScale;
		var updateLanguage = RatConfig.NameScan.Language != (Language)NameScanLanguage;
		var updateUiLanguage = RatConfig.UserInterface.Language != UiLanguage;

		// Save config
		RatConfig.NameScan.Enable = EnableNameScan;
		RatConfig.NameScan.EnableAuto = EnableAutoNameScan;
		RatConfig.NameScan.Language = (Language)NameScanLanguage;
		RatConfig.NameScan.Hotkey = NameScanHotkey;

		RatConfig.IconScan.Enable = EnableIconScan;
		RatConfig.IconScan.ScanRotatedIcons = ScanRotatedIcons;
		RatConfig.IconScan.UseCachedIcons = UseCachedIcons;
		RatConfig.IconScan.Hotkey = IconScanHotkey;

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

		RatConfig.Tracking.TarkovTracker.Token = TarkovTrackerToken.Trim();
		RatConfig.Tracking.TarkovTracker.ShowTeam = ShowTarkovTrackerTeam;
		RatConfig.Tracking.TarkovTracker.Backend = TarkovTrackerBackend;

		RatConfig.Overlay.Search.Enable = EnableIneractableOverlay;
		RatConfig.Overlay.Search.BlurBehind = BlurBehindSearch;
		RatConfig.Overlay.Search.Hotkey = InteractableOverlayHotkey;
		RatConfig.Overlay.Search.CloseHotkey = CloseOverlayHotkey;

		RatConfig.Hotkeys.OpenWiki = OpenWikiHotkey;
		RatConfig.Hotkeys.OpenTarkovDev = OpenTarkovDevHotkey;

		if (OverrideScreenConfig) {
			RatConfig.ScreenWidth = ScreenWidth;
			RatConfig.ScreenHeight = ScreenHeight;
			RatConfig.ScreenScale = ScreenScale;
			RatConfig.OverrideScreenConfig = true;
		}
		RatConfig.GameMode = GameMode;
		RatConfig.MinimizeToTray = MinimizeToTray;
		RatConfig.AlwaysOnTop = AlwaysOnTop;
		RatConfig.LogDebug = LogDebug;

		// Apply config
		PageSwitcher.Instance.Topmost = RatConfig.AlwaysOnTop;
		PageSwitcher.Instance.ResetWindowSize();
		await TarkovDevAPI.InitializeCache();
		if (updateTarkovTrackerToken || updateTarkovTrackerBackend) UpdateTarkovTrackerToken();
		if (updateUiLanguage) LocalizationService.SetLanguage(UiLanguage);
		if (updateResolution || updateLanguage || updateScreenOverride) RatScannerMain.Instance.SetupRatEye();

		RatEye.Config.LogDebug = RatConfig.LogDebug;
		RatScannerMain.Instance.HotkeyManager.RegisterHotkeys();

		// Save config to file
		Logger.LogInfo("Saving config...");
		RatConfig.SaveConfig();
		Logger.LogInfo("Config saved!");
		IsDirty = false;
		_suppressDirty = true;
		Notify();
	}

	private static void UpdateTarkovTrackerToken() {
		var token = RatConfig.Tracking.TarkovTracker.Token;
		if (token == "") return;
		RatScannerMain.Instance.TarkovTrackerDB.Token = RatConfig.Tracking.TarkovTracker.Token;
		var db = RatScannerMain.Instance.TarkovTrackerDB;
		if (db.TestToken(token)) {
			db.UpdateToken();
			return;
		}

		var visibleLength = (int)(token.Length * 0.25);
		token = token[..visibleLength] + string.Concat(Enumerable.Repeat(" *", token.Length - visibleLength));
		Logger.ShowWarning($"The TarkovTracker API Token does not seem to work.\n\n{token}");

		RatConfig.Tracking.TarkovTracker.Token = "";
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	internal virtual void OnPropertyChanged(string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
