using RatEye;
using RatScanner.Pages.InteractableOverlay.Services;
using RatScanner.Properties;
using RatScanner.Scan;
using RatScanner.View;
using RatStash;
using TarkovTask = RatScanner.TarkovDev.Json.TarkovTask;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Size = System.Drawing.Size;
using Timer = System.Threading.Timer;

namespace RatScanner;

public class RatScannerMain : INotifyPropertyChanged {
	internal static RatScannerMain Instance { get => field ??= new RatScannerMain(); private set; } = null!;

	internal readonly HotkeyManager HotkeyManager;
	private Timer? _tarkovTrackerDBRefreshTimer;
	private Timer? _scanExpiryTimer;
	private EftLogMonitor? _eftLogMonitor;

	private EftPlayerPosition? _playerPosition;

	/// <summary>
	/// The last player position seen from a screenshot, or null when there is
	/// none. Exposed so views can read it without owning the watcher.
	/// </summary>
	internal EftPlayerPosition? PlayerPosition => _playerPosition;

	/// <summary>
	/// Lock for name scanning
	/// </summary>
	/// <remarks>
	/// Lock order: 0
	/// </remarks>
	internal static object NameScanLock = new();

	/// <summary>
	/// Lock for icon scanning
	/// </summary>
	/// <remarks>
	/// Lock order: 1
	/// </remarks>
	internal static object IconScanLock = new();

	public TarkovTrackerDB TarkovTrackerDB;
	public LocalProgressStore LocalProgress;

	internal RatEyeEngine RatEyeEngine;

	public event PropertyChangedEventHandler? PropertyChanged;

	internal ItemQueue ItemScans = new();

	/// <summary>
	/// Items, quests and maps the user has picked, oldest first. Lives here rather
	/// than on the page so the selection outlives the component being unmounted by
	/// navigating to settings, and so the wiki and tarkov.dev hotkeys read the same
	/// answer the display does instead of a separate last-scan lookup.
	/// </summary>
	internal FixedSizeQueue<SearchResult> SelectedResults = new(10) {
		new SearchResult(new object[] {
			TarkovDevAPI.GetTasks().Random(),
			TarkovDevAPI.GetItems().Random(),
			TarkovDevAPI.GetMaps().Random(),
		}.Random(), 0)
	};

	/// <summary>
	/// What is on screen, or null when nothing has been picked yet.
	/// </summary>
	internal SearchResult? Selected => SelectedResults.Count > 0 ? SelectedResults.Last() : null;

	/// <summary>
	/// Brings a task the game reported onto the screen and says so.
	///
	/// The notification and the selection are one step: the message tells the
	/// player a quest changed without them touching anything, and opening it means
	/// the UI is already showing the objectives they may want to tick or the
	/// rewards they may want to claim.
	/// </summary>
	internal void ShowTaskFromLog(TarkovTask task, Logger.NotifyLevel level, string message) {
		Logger.Notify(level, message);

		// Enqueue ignores an entry equal to the current tail, so re-reporting the
		// task already on screen does not disturb it.
		SelectedResults.Enqueue(new SearchResult(task, 0));
	}

	private void OnSelectedResultsChanged() => OnPropertyChanged(nameof(Selected));

	/// <summary>
	/// A task the game reported. The notification always goes out; the task is
	/// only opened when the catalogue knows it, since there is nothing to show for
	/// an id RatScanner cannot resolve to a task.
	/// </summary>
	private void OnTaskReportedFromLog(TarkovTask? task, string message, Logger.NotifyLevel level) {
		if (task == null) {
			Logger.Notify(level, message);
			return;
		}

		ShowTaskFromLog(task, level, message);
	}

	public RatScannerMain() {
		Instance = this;

		// Remove old log
		Logger.Clear();

		Logger.LogInfo("----- RatScanner " + RatConfig.Version + " -----");
		Logger.LogInfo("Checking for updates...");
		CheckForUpdates();

		Logger.LogInfo($"Screen Info: {RatConfig.ScreenWidth}x{RatConfig.ScreenHeight} at {RatConfig.ScreenScale * 100}%");

		Logger.LogInfo("Initializing TarkovDev API...");

		// Try to load from offline cache first for faster startup
		if (TarkovDevAPI.TryInitializeCacheFromOffline()) {
			// Cache loaded from offline storage, queue background refresh
			Logger.LogInfo("Using offline cache for fast startup, refreshing in background...");
			_ = TarkovDevAPI.InitializeCache();
		} else {
			// No offline cache available, wait for network requests
			Logger.LogWarning("No complete offline cache available, fetching from network...");
			TarkovDevAPI.InitializeCache().Wait();
		}

		var items = TarkovDevAPI.GetItems();
		ItemScans.Enqueue(new DefaultItemScan(items[new Random().Next(items.Length)]));

		// Every scan path funnels through the queue, so this covers them all.
		ItemScans.Enqueued += OnItemScanEnqueued;
		SelectedResults.OnChange += OnSelectedResultsChanged;

		Logger.LogInfo("Initializing tarkov tracker database");
		TarkovTrackerDB = new TarkovTrackerDB();

		Logger.LogInfo("Initializing local progress store");
		LocalProgress = new LocalProgressStore();

		EftQuestTracker.TaskReported += OnTaskReportedFromLog;

		Logger.LogInfo("Initializing hotkey manager...");
		HotkeyManager = new HotkeyManager();
		HotkeyManager.UnregisterHotkeys();

		Logger.LogInfo("UI Ready!");

		Logger.LogInfo("Initializing RatEye...");
		SetupRatEye();

		new Thread(() => {
			Thread.Sleep(1000);
			Logger.LogInfo("Loading TarkovTracker data...");
			if (RatConfig.Tracking.TarkovTracker.Enable) {
				TarkovTrackerDB.Token = RatConfig.Tracking.TarkovTracker.Token;
				Logger.LogInfo("Loading TarkovTracker...");
				if (!TarkovTrackerDB.Init()) {
					Logger.ShowWarning("TarkovTracker API Token invalid!\n\nPlease provide a new token.");
					RatConfig.Tracking.TarkovTracker.Token = "";
					RatConfig.SaveConfig();
				}
			}

			Logger.LogInfo("Setting up timer routines...");
			_tarkovTrackerDBRefreshTimer = new Timer(RefreshTarkovTrackerDB, null, RatConfig.Tracking.TarkovTracker.RefreshTime, Timeout.Infinite);

			if (RatConfig.LogTracking.Enable) {
				Logger.LogInfo("Starting EFT log tracking...");
				_eftLogMonitor = new EftLogMonitor();
				_eftLogMonitor.DataReceived += OnEftLogData;
				_eftLogMonitor.Start();
			}

			if (RatConfig.LogTracking.TrackPlayerPosition) StartPlayerPositionTracking();

			Logger.LogInfo("Enabling hotkeys...");
			HotkeyManager.RegisterHotkeys();

			Logger.LogInfo("Ready!");
		}).Start();
	}

	/// <summary>
	/// Restarts log tracking after the settings changed. Safe to call when it is
	/// already stopped or running.
	/// </summary>
	internal void RestartEftLogTracking() {
		_eftLogMonitor?.Dispose();
		_eftLogMonitor = null;

		// The position tracker hangs off a folder rather than a log file, so it
		// follows the setting on its own rather than the monitor's lifetime.
		StopPlayerPositionTracking();

		if (!RatConfig.LogTracking.Enable) return;

		_eftLogMonitor = new EftLogMonitor();
		_eftLogMonitor.DataReceived += OnEftLogData;
		_eftLogMonitor.Start();

		if (RatConfig.LogTracking.TrackPlayerPosition) StartPlayerPositionTracking();
	}

	/// <summary>
	/// Starts watching for screenshots. Safe to call when already running.
	/// </summary>
	internal void StartPlayerPositionTracking() {
		if (_playerPosition != null) return;

		_playerPosition = new EftPlayerPosition();
		_playerPosition.PositionChanged += OnPlayerPositionChanged;
		_playerPosition.Start();
	}

	private void StopPlayerPositionTracking() {
		if (_playerPosition == null) return;

		_playerPosition.PositionChanged -= OnPlayerPositionChanged;
		_playerPosition.Dispose();
		_playerPosition = null;
	}

	/// <summary>
	/// Raised on a background thread whenever a new position is seen, so views
	/// showing a map can redraw the marker.
	/// </summary>
	internal event Action? PlayerPositionChanged;

	private void OnPlayerPositionChanged() => PlayerPositionChanged?.Invoke();

	/// <summary>
	/// The past log sessions that could be replayed, newest first. Empty when the
	/// logs folder cannot be found, which is a normal state.
	/// </summary>
	internal List<EftLogHistory.Session> PastLogSessions() => EftLogHistory.Sessions();

	/// <summary>
	/// Replays a past session's logs through the same parsers the live monitor
	/// feeds, so quest progress from raids that already happened is picked up.
	///
	/// Only quest tracking is replayed: the raid lifecycle would announce a raid
	/// that finished long ago, and the position marker has no timestamps to place.
	/// </summary>
	/// <param name="session">Oldest session to read.</param>
	/// <param name="includeNewer">
	/// Whether to also read everything more recent than
	/// <paramref name="session"/>.
	/// </param>
	internal void ReplayPastSession(EftLogHistory.Session session, bool includeNewer = true) {
		if (!RatConfig.LogTracking.TrackQuests) return;

		var sessions = EftLogHistory.Sessions();

		var last = sessions.FindIndex(s => s.Equals(session));
		if (last < 0) return;

		// Newest first. The sessions to read are those at or newer than the chosen
		// one, which are the entries at the head of the list down to it.
		var first = includeNewer ? 0 : last;

		// Progress is recorded, but nothing is announced or opened: a past session
		// can hold dozens of quests, and popping a notification for each while
		// dragging the selection around would bury the UI.
		EftQuestTracker.Replaying = true;
		try {
			for (var i = first; i <= last; i++) {
				foreach (var (type, data) in EftLogHistory.Read(sessions[i])) {
					if (type != EftLogType.Notifications) continue;
					EftQuestTracker.Consume(data);
				}
			}
		} finally {
			EftQuestTracker.Replaying = false;
		}

		Logger.LogInfo($"Replayed {last - first + 1} past EFT session(s) up to and including {session.Label}.");
	}

	private void OnEftLogData(EftLogType type, string data) {
		switch (type) {
			case EftLogType.Application:
				if (RatConfig.LogTracking.TrackGameMode) EftGameModeDetector.Consume(data);
				if (RatConfig.LogTracking.TrackRaidLifecycle) EftRaidLifecycle.Consume(data);
				break;

			case EftLogType.Notifications:
				if (RatConfig.LogTracking.TrackQuests) EftQuestTracker.Consume(data);
				break;
		}
	}

	private static void CheckForUpdates() {
		var mostRecentVersion = ApiManager.GetResource(ApiManager.ResourceType.ClientVersion);
		if (RatConfig.Version == mostRecentVersion) return;
		Logger.LogInfo("A new version is available: " + mostRecentVersion);

		var forceVersions = ApiManager.GetResource(ApiManager.ResourceType.ClientForceUpdateVersions);
		if (forceVersions.Contains($"[{RatConfig.Version}]")) {
			UpdateRatScanner();
			return;
		}

		var message = "Version " + mostRecentVersion + " is available!\n";
		message += "You are using: " + RatConfig.Version + "\n\n";
		message += "Do you want to install it now?";
		var temp = new Window() { Visibility = Visibility.Hidden };
		temp.Show();
		var result = MessageBox.Show(temp, message, "Rat Scanner Updater", MessageBoxButton.YesNo);
		if (result == MessageBoxResult.Yes) UpdateRatScanner();
	}

	private static void UpdateRatScanner() {
		if (!File.Exists(RatConfig.Paths.Updater)) {
			Logger.LogWarning(RatConfig.Paths.Updater + " could not be found!");
			try {
				var updaterLink = ApiManager.GetResource(ApiManager.ResourceType.UpdaterLink);
				ApiManager.DownloadFile(updaterLink, RatConfig.Paths.Updater);
			} catch (Exception e) {
				Logger.LogError("Unable to download updater, please update manually.", e);
				return;
			}
		}

		ProcessStartInfo startInfo = new(RatConfig.Paths.Updater) {
			UseShellExecute = true
		};
		startInfo.ArgumentList.Add("--start");
		startInfo.ArgumentList.Add("--update");
		_ = Process.Start(startInfo);
		Environment.Exit(0);
	}

	[MemberNotNull(nameof(RatEyeEngine))]
	internal void SetupRatEye() {
		Config.LogDebug = RatConfig.LogDebug;
		Config.Path.LogFile = "RatEyeLog.txt";
		Config.Path.TesseractLibSearchPath = AppDomain.CurrentDomain.BaseDirectory;
		RatEyeEngine = new RatEyeEngine(GetRatEyeConfig(), RatStashDatabaseFromTarkovDev());
	}

	private static Config GetRatEyeConfig(bool highlighted = true) {
		return new Config() {
			PathConfig = new Config.Path() {
				TrainedData = RatConfig.Paths.TrainedData,
				StaticIcons = RatConfig.Paths.StaticIcon,
			},
			ProcessingConfig = new Config.Processing() {
				Scale = Config.Processing.Resolution2Scale(RatConfig.ScreenWidth, RatConfig.ScreenHeight),
				Language = RatConfig.NameScan.Language,
				IconConfig = new Config.Processing.Icon() {
					UseStaticIcons = true,
					ScanMode = Config.Processing.Icon.ScanModes.TemplateMatching,
				},
				InventoryConfig = new Config.Processing.Inventory() {
					OptimizeHighlighted = highlighted,
				},
				InspectionConfig = new Config.Processing.Inspection() {
					Marker = Resources.icon_search,
				},
			},
		};
	}

	private static Database RatStashDatabaseFromTarkovDev() {
		List<Item> rsItems = [];
		foreach (var i in TarkovDevAPI.GetItems()) {
			rsItems.Add(new Item() {
				Id = i.Id,
				Name = i.Name,
				ShortName = i.ShortName,

			});
		}
		return Database.FromItems(rsItems);
	}

	/// <summary>
	/// Perform a name scan at the give position
	/// </summary>
	/// <param name="position">Position on the screen at which to perform the scan</param>
	internal void NameScan(Vector2 position) {
		lock (NameScanLock) {
			// Wait for game ui to update the click
			Thread.Sleep(50);

			// Get raw screenshot which includes the icon and text
			var markerScanSize = RatConfig.NameScan.MarkerScanSize;
			var sizeWidth = markerScanSize + RatConfig.NameScan.TextWidth;
			var sizeHeight = markerScanSize;

			position -= new Vector2(markerScanSize / 2, markerScanSize / 2);

			var screenshot = GetScreenshot(position, new Size(sizeWidth, sizeHeight));

			// Scan the item
			var inspection = RatEyeEngine.NewInspection(screenshot);

			if (!inspection.ContainsMarker || inspection.Item == null) return;

			var scale = RatEyeEngine.Config.ProcessingConfig.Scale;
			var marker = RatEyeEngine.Config.ProcessingConfig.InspectionConfig.Marker;
			var toolTipPosition = inspection.MarkerPosition;
			toolTipPosition += new Vector2(-(int)(marker.Width * scale), (int)(marker.Height * scale));
			toolTipPosition += position;

			ItemNameScan tempNameScan = new(
				inspection,
				toolTipPosition,
				RatConfig.ToolTip.Duration);

			ItemScans.Enqueue(tempNameScan);

			RefreshOverlay();
		}
	}

	/// <summary>
	/// Perform a name scan over the entire active screen
	/// </summary>
	internal void NameScanScreen(object? _ = null) {
		lock (NameScanLock) {
			Logger.LogDebug("Name scanning screen");
			var mousePosition = UserActivityHelper.GetMousePosition();
			var bounds = Screen.AllScreens.First(screen => screen.Bounds.Contains(mousePosition)).Bounds;

			Vector2 position = new(bounds.X, bounds.Y);
			var screenshot = GetScreenshot(position, bounds.Size);

			// Scan the item
			var multiInspection = RatEyeEngine.NewMultiInspection(screenshot);

			if (multiInspection.Inspections.Count == 0) return;

			foreach (var inspection in multiInspection.Inspections) {
				var scale = RatEyeEngine.Config.ProcessingConfig.Scale;
				var toolTipPosition = inspection.MarkerPosition;
				toolTipPosition += position;
				var marker = RatEyeEngine.Config.ProcessingConfig.InspectionConfig.Marker;
				toolTipPosition += new Vector2(0, (int)(marker.Height * scale));

				ItemNameScan tempNameScan = new(
						inspection,
						toolTipPosition,
						RatConfig.ToolTip.Duration);

				ItemScans.Enqueue(tempNameScan);
			}
			RefreshOverlay();
		}
	}

	/// <summary>
	/// Perform a icon scan at the given position
	/// </summary>
	/// <param name="position">Position on the screen at which to perform the scan</param>
	/// <returns><see langword="true"/> if a item was scanned successfully</returns>
	internal void IconScan(Vector2 position) {
		lock (IconScanLock) {
			Logger.LogDebug("Icon scanning at: " + position);
			var x = position.X - (RatConfig.IconScan.ScanWidth / 2);
			var y = position.Y - (RatConfig.IconScan.ScanHeight / 2);

			Vector2 screenshotPosition = new(x, y);
			Size size = new(RatConfig.IconScan.ScanWidth, RatConfig.IconScan.ScanHeight);
			var screenshot = GetScreenshot(screenshotPosition, size);

			// Scan the item
			var inventory = RatEyeEngine.NewInventory(screenshot);
			var icon = inventory.LocateIcon();

			if (icon?.DetectionConfidence <= 0 || icon?.Item == null) return;

			var toolTipPosition = position;
			toolTipPosition += icon.Position + icon.ItemPosition;
			toolTipPosition -= new Vector2(RatConfig.IconScan.ScanWidth, RatConfig.IconScan.ScanHeight) / 2;

			ItemIconScan tempIconScan = new(icon, toolTipPosition, RatConfig.ToolTip.Duration);

			ItemScans.Enqueue(tempIconScan);
			RefreshOverlay();
		}
	}

	// Returns the ruff screenshot
	private static Bitmap GetScreenshot(Vector2 vector2, Size size) {
		Bitmap bmp = new(size.Width, size.Height, PixelFormat.Format24bppRgb);

		try {
			using var gfx = Graphics.FromImage(bmp);
			gfx.CopyFromScreen(vector2.X, vector2.Y, 0, 0, size, CopyPixelOperation.SourceCopy);
		} catch (Exception e) {
			Logger.LogWarning("Unable to capture screenshot", e);
		}

		return bmp;
	}

	private void RefreshTarkovTrackerDB(object? o = null) {
		Logger.LogInfo("Refreshing TarkovTracker DB...");
		_ = TarkovTrackerDB.Init();
		_ = _tarkovTrackerDBRefreshTimer.Change(RatConfig.Tracking.TarkovTracker.RefreshTime, Timeout.Infinite);
	}

	/// <summary>
	/// Re-applies the scanning overlay setting, so toggling it off in the settings
	/// takes effect on an overlay that is currently on screen rather than waiting
	/// for the next scan.
	/// </summary>
	internal void ApplyScanningOverlaySetting() => RefreshOverlay();

	private void RefreshOverlay(object? o = null) {
		SetOverlayVisible(HasLiveScan());

		OnPropertyChanged();
	}

	/// <summary>
	/// Arms a one-shot refresh for when the newest scan expires, so the overlay
	/// hides itself without polling. A scan's lifetime is already known, so the
	/// expiry can be scheduled rather than polled for.
	/// </summary>
	private void ScheduleScanExpiry() {
		var latest = 0L;
		foreach (var scan in ItemScans) {
			if (scan != null && scan.DissapearAt > latest) latest = scan.DissapearAt;
		}

		// A non-positive dueTime is not schedulable: Change rejects anything under
		// -1 and treats 0 as immediate, which would spin this method.
		if (latest != 0 && latest <= DateTimeOffset.Now.ToUnixTimeMilliseconds()) {
			RefreshOverlay();
			return;
		}

		lock (ScanExpiryLock) {
			_scanExpiryTimer ??= new Timer(_ => OnScanExpired(), null, Timeout.Infinite, Timeout.Infinite);
			_scanExpiryTimer.Change(latest == 0 ? Timeout.Infinite : latest - DateTimeOffset.Now.ToUnixTimeMilliseconds(),
				Timeout.Infinite);
		}
	}

	/// <summary>
	/// Re-arms if a scan arrived while this was pending, which can happen when it
	/// was enqueued from another thread after that enqueue's own re-arm.
	/// </summary>
	private void OnScanExpired() {
		RefreshOverlay();

		if (HasLiveScan()) ScheduleScanExpiry();
	}

	private void OnItemScanEnqueued(object? sender, ItemScan scan) => ScheduleScanExpiry();

	private readonly object ScanExpiryLock = new();

	// The scan timer is a background thread and a window can only be shown or
	// hidden on the UI thread. Fire and forget, so the cancellation that comes
	// with the dispatcher tearing down at close does not escape and break the
	// debugger.
	private void SetOverlayVisible(bool visible) {
		if (!RatConfig.Overlay.Scanning.Enable) {
			SetOverlayVisibleRaw(false);
			return;
		}

		SetOverlayVisibleRaw(visible);
	}

	private void SetOverlayVisibleRaw(bool visible) {
		var overlay = BlazorUI.BlazorOverlay;
		if (overlay == null) return;

		var dispatcher = System.Windows.Application.Current?.Dispatcher;
		if (dispatcher == null || dispatcher.CheckAccess()) {
			overlay.SetVisible(visible);
			return;
		}

		_ = dispatcher.BeginInvoke(() => overlay.SetVisible(visible));
	}

	private bool HasLiveScan() {
		var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

		foreach (var scan in ItemScans) {
			if (scan != null && scan.DissapearAt > now) return true;
		}

		return false;
	}
	protected virtual void OnPropertyChanged(string propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
