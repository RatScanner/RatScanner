using RatEye;
using RatScanner.Scan;
using RatStash;
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
	private static RatScannerMain _instance = null!;
	internal static RatScannerMain Instance => _instance ??= new RatScannerMain();

	internal readonly HotkeyManager HotkeyManager;

	private Timer? _marketDBRefreshTimer;
	private Timer? _tarkovTrackerDBRefreshTimer;
	private Timer? _scanRefreshTimer;

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

	internal RatEyeEngine RatEyeEngine;

	public event PropertyChangedEventHandler? PropertyChanged;

	internal ItemQueue ItemScans = new();

	public RatScannerMain() {
		_instance = this;

		// Remove old log
		Logger.Clear();

		Logger.LogInfo("----- RatScanner " + RatConfig.Version + " -----");
		Logger.LogInfo($"Screen Info: {RatConfig.ScreenWidth}x{RatConfig.ScreenHeight} at {RatConfig.ScreenScale * 100}%");

		Logger.LogInfo("Initializing TarkovDev API...");
		TarkovDevAPI.InitializeCache().Wait();

		ItemScans.Enqueue(new DefaultItemScan(TarkovDevAPI.GetItems()[new Random().Next(TarkovDevAPI.GetItems().Length)]));

		Logger.LogInfo("Initializing tarkov tracker database");
		TarkovTrackerDB = new TarkovTrackerDB();

		Logger.LogInfo("Initializing hotkey manager...");
		HotkeyManager = new HotkeyManager();
		HotkeyManager.UnregisterHotkeys();

		Logger.LogInfo("UI Ready!");

		Logger.LogInfo("Initializing RatEye...");
		SetupRatEye();

		new Thread(() => {
			Thread.Sleep(1000);
			Logger.LogInfo("Checking for updates...");

			CheckForUpdates();

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
			_scanRefreshTimer = new Timer(RefreshOverlay, null, 1000, 100);

			Logger.LogInfo("Enabling hotkeys...");
			HotkeyManager.RegisterHotkeys();

			Logger.LogInfo("Ready!");
		}).Start();
	}

	private void CheckForUpdates() {
		string mostRecentVersion = ApiManager.GetResource(ApiManager.ResourceType.ClientVersion);

		if (!ShouldUpdate(mostRecentVersion)) return;

		Logger.LogInfo("A new version is available: " + mostRecentVersion);

		string forceVersions = ApiManager.GetResource(ApiManager.ResourceType.ClientForceUpdateVersions);
		if (forceVersions.Contains($"[{RatConfig.Version}]")) {
			UpdateRatScanner();
			return;
		}

		string message = "Version " + mostRecentVersion + " is available!\n";
		message += "You are using: " + RatConfig.Version + "\n\n";
		message += "Do you want to install it now?";
		MessageBoxResult result = MessageBox.Show(message, "Rat Scanner Updater", MessageBoxButton.YesNo);
		if (result == MessageBoxResult.Yes) UpdateRatScanner();
	}

	private static bool ShouldUpdate(string mostRecentVersion) {
		string currentVersion = RatConfig.Version;
		if (currentVersion == mostRecentVersion) return false;

		string[] versions = currentVersion.Split(".");
		string[] mostRecentVersions = mostRecentVersion.Split(".");

		bool shouldUpdate = false;
		for (int i = 0; i < versions.Length; i++) {
			int version = Int32.Parse(versions[i]);
			int recentVersion = Int32.Parse(mostRecentVersions[i]);

			if (version < recentVersion) {
				shouldUpdate = true;
			}
		}

		return shouldUpdate;
	}

	private void UpdateRatScanner() {
		if (!File.Exists(RatConfig.Paths.Updater)) {
			Logger.LogWarning(RatConfig.Paths.Updater + " could not be found!");
			try {
				string updaterLink = ApiManager.GetResource(ApiManager.ResourceType.UpdaterLink);
				ApiManager.DownloadFile(updaterLink, RatConfig.Paths.Updater);
			} catch (Exception e) {
				Logger.LogError("Unable to download updater, please update manually.", e);
				return;
			}
		}

		ProcessStartInfo startInfo = new(RatConfig.Paths.Updater);
		startInfo.UseShellExecute = true;
		startInfo.ArgumentList.Add("--start");
		startInfo.ArgumentList.Add("--update");
		Process.Start(startInfo);
		Environment.Exit(0);
	}

	[MemberNotNull(nameof(RatEyeEngine))]
	internal void SetupRatEye() {
		Config.LogDebug = RatConfig.LogDebug;
		Config.Path.LogFile = "RatEyeLog.txt";
		Config.Path.TesseractLibSearchPath = AppDomain.CurrentDomain.BaseDirectory;
		RatEyeEngine = new RatEyeEngine(GetRatEyeConfig(), RatStashDatabaseFromTarkovDev());
	}

	private RatEye.Config GetRatEyeConfig(bool highlighted = true) {
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
					MarkerThreshold = 0.9f,
				},
			},
		};
	}

	private Database RatStashDatabaseFromTarkovDev() {
		List<Item> rsItems = new();
		foreach (TarkovDev.GraphQL.Item i in TarkovDevAPI.GetItems()) {
			rsItems.Add(new RatStash.Item() {
				Id = i.Id,
				Name = i.Name,
				ShortName = i.ShortName,

			});
		}
		return RatStash.Database.FromItems(rsItems);
	}

	/// <summary>
	/// Perform a name scan at the give position
	/// </summary>
	/// <param name="position">Position on the screen at which to perform the scan</param>
	internal void NameScan(Vector2 position) {
		lock (NameScanLock) {
			Logger.LogDebug("Name scanning at: " + position);
			// Wait for game ui to update the click
			Thread.Sleep(50);

			// Get raw screenshot which includes the icon and text
			int markerScanSize = RatConfig.NameScan.MarkerScanSize;
			int sizeWidth = markerScanSize + RatConfig.NameScan.TextWidth;
			int sizeHeight = markerScanSize;

			position -= new Vector2(markerScanSize / 2, markerScanSize / 2);

			Bitmap screenshot = GetScreenshot(position, new Size(sizeWidth, sizeHeight));

			// Scan the item
			RatEye.Processing.Inspection inspection = RatEyeEngine.NewInspection(screenshot);

			if (!inspection.ContainsMarker || inspection.Item == null) return;

			float scale = RatEyeEngine.Config.ProcessingConfig.Scale;
			Bitmap marker = RatEyeEngine.Config.ProcessingConfig.InspectionConfig.Marker;
			Vector2 toolTipPosition = inspection.MarkerPosition;
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
			Vector2 mousePosition = UserActivityHelper.GetMousePosition();
			Rectangle bounds = Screen.AllScreens.First(screen => screen.Bounds.Contains(mousePosition)).Bounds;

			Vector2 position = new(bounds.X, bounds.Y);
			Bitmap screenshot = GetScreenshot(position, bounds.Size);

			// Scan the item
			RatEye.Processing.MultiInspection multiInspection = RatEyeEngine.NewMultiInspection(screenshot);

			if (multiInspection.Inspections.Count == 0) return;

			foreach (RatEye.Processing.Inspection? inspection in multiInspection.Inspections) {
				float scale = RatEyeEngine.Config.ProcessingConfig.Scale;
				Vector2 toolTipPosition = inspection.MarkerPosition;
				toolTipPosition += position;
				Bitmap marker = RatEyeEngine.Config.ProcessingConfig.InspectionConfig.Marker;
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
			int x = position.X - RatConfig.IconScan.ScanWidth / 2;
			int y = position.Y - RatConfig.IconScan.ScanHeight / 2;

			Vector2 screenshotPosition = new(x, y);
			Size size = new(RatConfig.IconScan.ScanWidth, RatConfig.IconScan.ScanHeight);
			Bitmap screenshot = GetScreenshot(screenshotPosition, size);

			// Scan the item
			RatEye.Processing.Inventory inventory = RatEyeEngine.NewInventory(screenshot);
			RatEye.Processing.Icon? icon = inventory.LocateIcon();

			if (icon?.DetectionConfidence <= 0 || icon?.Item == null) return;

			Vector2 toolTipPosition = position;
			toolTipPosition += icon.Position + icon.ItemPosition;
			toolTipPosition -= new Vector2(RatConfig.IconScan.ScanWidth, RatConfig.IconScan.ScanHeight) / 2;

			ItemIconScan tempIconScan = new(icon, toolTipPosition, RatConfig.ToolTip.Duration);

			ItemScans.Enqueue(tempIconScan);
			RefreshOverlay();
		}
	}

	// Returns the ruff screenshot
	private Bitmap GetScreenshot(Vector2 vector2, Size size) {
		Bitmap bmp = new(size.Width, size.Height, PixelFormat.Format24bppRgb);

		try {
			using Graphics gfx = Graphics.FromImage(bmp);
			gfx.CopyFromScreen(vector2.X, vector2.Y, 0, 0, size, CopyPixelOperation.SourceCopy);
		} catch (Exception e) {
			Logger.LogWarning("Unable to capture screenshot", e);
		}

		return bmp;
	}

	private void RefreshTarkovTrackerDB(object? o = null) {
		Logger.LogInfo("Refreshing TarkovTracker DB...");
		TarkovTrackerDB.Init();
		_tarkovTrackerDBRefreshTimer.Change(RatConfig.Tracking.TarkovTracker.RefreshTime, Timeout.Infinite);
	}
	private void RefreshOverlay(object? o = null) {
		OnPropertyChanged();
	}

	protected virtual void OnPropertyChanged(string propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
