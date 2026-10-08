using RatScanner.TarkovDev.Json;
using RatStash;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RatScanner.ViewModel;

/// <summary>
/// Trader standing the player reported by hand. A zero level means "not set",
/// and a null reputation means "not set", so each half of the gate can be left
/// unknown on its own.
/// </summary>
internal sealed class ManualTraderStanding {
	public int Level { get; set; }

	/// <summary>
	/// Standing on the trader's own scale. Fractional, because tarkov.dev publishes
	/// loyalty thresholds like 0.5, 2.5 and 6.5, so a whole number cannot describe
	/// where the player actually sits on the track.
	/// </summary>
	public double? Reputation { get; set; }
}

/// <summary>
/// One game mode's manually entered player profile, held while the tracking
/// settings page is open. The values are staged here and written to the progress
/// store on Save, so an unsaved edit does not change what the quest gates read.
/// </summary>
internal sealed class ManualProfile {
	/// <summary>PMC level the player reports by hand. Zero means "not set".</summary>
	public int PlayerLevel { get; set; }

	/// <summary>Which side the player fights for: "USEC", "BEAR" or empty.</summary>
	public string PlayerFaction { get; set; } = "";

	/// <summary>
	/// Per-trader standing the player reports by hand, keyed by trader id. Null
	/// members mean "not set" and leave that half of the gate unknown.
	/// </summary>
	public Dictionary<string, ManualTraderStanding> ManualTraders { get; set; } = new(StringComparer.Ordinal);
}

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

	public RatConfig.ProgressSource ProgressSource { get; set; }

	/// <summary>
	/// The manually entered profile for each game mode, staged until Save. Level,
	/// faction and trader standing differ per mode, so they cannot be a single
	/// shared value the way the config settings are.
	/// </summary>
	public Dictionary<GameMode, ManualProfile> ManualProfiles { get; set; } = new() {
		[GameMode.Regular] = new(),
		[GameMode.Pve] = new(),
		[GameMode.PvpSeason] = new(),
	};

	// TarkovTracker Specific Tracking Settings
	public string TarkovTrackerToken { get; set; }

	public bool ShowTarkovTrackerTeam { get; set; }

	public RatConfig.TarkovTrackerBackend TarkovTrackerBackend { get; set; }

	// Interactable Overlay
	public bool EnableIneractableOverlay { get; set; }
	public bool BlurBehindSearch { get; set; }
	public Hotkey InteractableOverlayHotkey { get; set; }
	public Hotkey CloseOverlayHotkey { get; set; }

	public bool EnableScanningOverlay { get; set; }

	// EFT log tracking
	public bool EnableLogTracking { get; set; }
	public bool LogTrackQuests { get; set; }
	public bool LogTrackRaidLifecycle { get; set; }
	public bool LogTrackGameMode { get; set; }
	public bool LogTrackPlayerPosition { get; set; }
	public string LogPathOverride { get; set; } = "";
	public string PositionMapFallback { get; set; } = "56f40101d2720b2a4d8b45d6";

	// Application hotkeys
	public Hotkey OpenWikiHotkey { get; set; }
	public Hotkey OpenTarkovDevHotkey { get; set; }
	public Hotkey OpenMapHotkey { get; set; }

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
		ProgressSource = RatConfig.Tracking.Source;

		// The manual overrides live in the progress store rather than in the INI
		// config, since they are per game mode and belong with the rest of the
		// progress. Every mode is read, not just the active one, so the settings
		// tabs can show all of them at once and none gets overwritten on Save.
		ManualProfiles = new Dictionary<GameMode, ManualProfile>();
		var store = RatScannerMain.Instance?.LocalProgress;
		foreach (var mode in Enum.GetValues<GameMode>()) {
			var profile = new ManualProfile();
			ManualProfiles[mode] = profile;

			var local = store?.GetProgress(mode);
			if (local is null) continue;

			profile.PlayerLevel = local.PlayerLevel ?? 0;
			profile.PlayerFaction = local.PmcFaction ?? "";
			foreach (var trader in local.Traders) {
				profile.ManualTraders[trader.Id] = new ManualTraderStanding {
					Level = trader.Level ?? 0,
					Reputation = trader.Reputation,
				};
			}
		}

		TarkovTrackerToken = RatConfig.Tracking.TarkovTracker.Token;
		ShowTarkovTrackerTeam = RatConfig.Tracking.TarkovTracker.ShowTeam;
		TarkovTrackerBackend = RatConfig.Tracking.TarkovTracker.Backend;

		EnableIneractableOverlay = RatConfig.Overlay.Search.Enable;
		BlurBehindSearch = RatConfig.Overlay.Search.BlurBehind;
		InteractableOverlayHotkey = new Hotkey(RatConfig.Overlay.Search.Hotkey);
		CloseOverlayHotkey = new Hotkey(RatConfig.Overlay.Search.CloseHotkey);

		EnableScanningOverlay = RatConfig.Overlay.Scanning.Enable;

		EnableLogTracking = RatConfig.LogTracking.Enable;
		LogTrackQuests = RatConfig.LogTracking.TrackQuests;
		LogTrackRaidLifecycle = RatConfig.LogTracking.TrackRaidLifecycle;
		LogTrackGameMode = RatConfig.LogTracking.TrackGameMode;
		LogTrackPlayerPosition = RatConfig.LogTracking.TrackPlayerPosition;
		LogPathOverride = RatConfig.LogTracking.LogsPathOverride;
		PositionMapFallback = RatConfig.LogTracking.PositionMapFallback;

		OpenWikiHotkey = new Hotkey(RatConfig.Hotkeys.OpenWiki);
		OpenTarkovDevHotkey = new Hotkey(RatConfig.Hotkeys.OpenTarkovDev);
		OpenMapHotkey = new Hotkey(RatConfig.Hotkeys.OpenMap);

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
	/// Raised after the game mode actually changes, so open views can re-resolve
	/// anything they took from the previous mode's catalogue.
	/// </summary>
	public static event Action<GameMode>? GameModeChanged;

	/// <summary>
	/// The single entry point for changing game mode, used by both the title bar
	/// chip and the settings page. Applies the mode, persists it, refreshes the
	/// catalogue and tells listeners, so neither surface can end up out of step
	/// with the other.
	/// </summary>
	public async Task SetGameMode(GameMode mode) {
		if (RatConfig.GameMode == mode) return;

		RatConfig.GameMode = mode;
		GameMode = mode;
		RatConfig.SaveConfig();
		LocalProgressStore.NotifyChanged();

		try {
			await TarkovDevAPI.InitializeCache();
		} catch (Exception e) {
			Logger.LogWarning("Failed to refresh tarkov.dev cache after game mode switch", e);
		}

		QuestObjectiveEntry.InvalidateItemLookup();
		RatScanner.TaskExtensions.InvalidateItemLookup();

		// Not routed through MarkDirty: a switch is applied and saved at once, so
		// the settings page has nothing left to save. Notify still re-renders it so
		// its dropdown shows the new mode.
		Notify();

		GameModeChanged?.Invoke(mode);
	}

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
		RatConfig.Tracking.Source = ProgressSource;

		// Written straight to the progress store instead of being staged on the
		// config, because they are per game mode and are read back through it.
		// Every mode is written, not just the active one, so saving from any tab
		// does not discard the others, and each mode is written whole so a
		// trader cleared in the UI is removed rather than left behind.
		var progress = RatScannerMain.Instance?.LocalProgress;
		if (progress is not null) {
			foreach (var (mode, profile) in ManualProfiles) {
				progress.SetProfile(
					mode,
					profile.PlayerLevel > 0 ? profile.PlayerLevel : null,
					profile.PlayerFaction,
					ToStoreStanding(profile));
			}
		}

		RatConfig.Tracking.TarkovTracker.Token = TarkovTrackerToken.Trim();
		RatConfig.Tracking.TarkovTracker.ShowTeam = ShowTarkovTrackerTeam;
		RatConfig.Tracking.TarkovTracker.Backend = TarkovTrackerBackend;

		RatConfig.Overlay.Search.Enable = EnableIneractableOverlay;
		RatConfig.Overlay.Search.BlurBehind = BlurBehindSearch;
		RatConfig.Overlay.Search.Hotkey = InteractableOverlayHotkey;
		RatConfig.Overlay.Search.CloseHotkey = CloseOverlayHotkey;

		RatConfig.Overlay.Scanning.Enable = EnableScanningOverlay;

		RatConfig.LogTracking.Enable = EnableLogTracking;
		RatConfig.LogTracking.TrackQuests = LogTrackQuests;
		RatConfig.LogTracking.TrackRaidLifecycle = LogTrackRaidLifecycle;
		RatConfig.LogTracking.TrackGameMode = LogTrackGameMode;
		RatConfig.LogTracking.TrackPlayerPosition = LogTrackPlayerPosition;
		RatConfig.LogTracking.LogsPathOverride = LogPathOverride.Trim();
		RatConfig.LogTracking.PositionMapFallback = PositionMapFallback;

		RatConfig.Hotkeys.OpenWiki = OpenWikiHotkey;
		RatConfig.Hotkeys.OpenTarkovDev = OpenTarkovDevHotkey;
		RatConfig.Hotkeys.OpenMap = OpenMapHotkey;

		if (OverrideScreenConfig) {
			RatConfig.ScreenWidth = ScreenWidth;
			RatConfig.ScreenHeight = ScreenHeight;
			RatConfig.ScreenScale = ScreenScale;
			RatConfig.OverrideScreenConfig = true;
		}
		var gameModeChanged = RatConfig.GameMode != GameMode;

		RatConfig.MinimizeToTray = MinimizeToTray;
		RatConfig.AlwaysOnTop = AlwaysOnTop;
		RatConfig.LogDebug = LogDebug;

		// Apply config
		PageSwitcher.Instance.Topmost = RatConfig.AlwaysOnTop;
		PageSwitcher.Instance.ResetWindowSize();
		if (updateTarkovTrackerToken || updateTarkovTrackerBackend) UpdateTarkovTrackerToken();
		if (updateUiLanguage) LocalizationService.SetLanguage(UiLanguage);
		if (updateResolution || updateLanguage || updateScreenOverride) RatScannerMain.Instance.SetupRatEye();

		// The scanning overlay may have just been switched off while it was on
		// screen, so the new setting is applied rather than waiting for the next scan.
		RatScannerMain.Instance.ApplyScanningOverlaySetting();

		RatEye.Config.LogDebug = RatConfig.LogDebug;
		RatScannerMain.Instance.HotkeyManager.RegisterHotkeys();

		// Restarted unconditionally so a changed logs path or a freshly enabled
		// toggle takes effect without needing the app restarted.
		RatScannerMain.Instance.RestartEftLogTracking();

		// Save config to file
		Logger.LogInfo("Saving config...");
		RatConfig.SaveConfig();
		Logger.LogInfo("Config saved!");
		IsDirty = false;
		_suppressDirty = true;

		// Last, so the rest of the save is already applied. Routed through
		// SetGameMode so the chip picks the change up like any other listener.
		if (gameModeChanged) await SetGameMode(GameMode);
		else Notify();
	}

	// A zero level and a null reputation both mean "not set", so they are handed
	// over as nulls rather than as the placeholder values the UI holds.
	private static IEnumerable<(string Id, int? Level, double? Reputation)> ToStoreStanding(ManualProfile profile) {
		foreach (var (id, trader) in profile.ManualTraders) {
			yield return (id, trader.Level > 0 ? trader.Level : null, trader.Reputation);
		}
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
