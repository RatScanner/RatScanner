using RatEye;
using RatScanner.View;
using RatTracking;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using RatLib.Scan;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.MessageBox;
using Size = System.Drawing.Size;
using Timer = System.Threading.Timer;

namespace RatScanner;

public class RatScannerMain : INotifyPropertyChanged
{
	private static RatScannerMain _instance = null;
	internal static RatScannerMain Instance => _instance ??= new RatScannerMain();

	internal readonly HotkeyManager HotkeyManager;

	private readonly BlazorOverlay _blazorOverlay;

	private ItemScan _currentItemScan;

	private Timer _marketDBRefreshTimer;
	private Timer _tarkovTrackerDBRefreshTimer;
	private Timer _scanRefreshTimer;
	private Timer _nameScanTimer;

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

	internal MarketDB MarketDB;
	internal ProgressDB ProgressDB;
	public TarkovTrackerDB TarkovTrackerDB;

	internal RatEyeEngine RatEyeEngine;
	internal Config RatEyeConfig;

	public event PropertyChangedEventHandler PropertyChanged;

	internal ItemScan CurrentItemScan
	{
		get => _currentItemScan;
		set
		{
			_currentItemScan = value;
			OnPropertyChanged();
		}
	}

	public RatScannerMain()
	{
		_instance = this;

		// Remove old log
		Logger.Clear();

		Logger.LogInfo("----- RatScanner " + RatConfig.Version + " -----");
		Logger.LogInfo("Starting RatScanner...");

		Logger.LogInfo("Checking for updates...");
		CheckForUpdates();

		Logger.LogInfo("Loading config...");
		RatConfig.LoadConfig();

		Logger.LogInfo("Checking for item data updates...");
		CheckForItemDataUpdates();

		// Check for TarkovTracker data
		TarkovTrackerDB = new TarkovTrackerDB();
		if (RatConfig.Tracking.TarkovTracker.Enable)
		{
			TarkovTrackerDB.Token = RatConfig.Tracking.TarkovTracker.Token;
			Logger.LogInfo("Loading TarkovTracker...");
			if (!TarkovTrackerDB.Init())
			{
				Logger.ShowWarning("TarkovTracker API Token invalid!\n\nPlease provide a new token.");
				RatConfig.Tracking.TarkovTracker.Token = "";
				RatConfig.SaveConfig();
			}
		}

		// Grab quest and hideout requirements from tarkovdata
		Logger.LogInfo("Loading progress data...");
		ProgressDB = new ProgressDB();
		ProgressDB.Init();

		Logger.LogInfo("Loading price data...");
		MarketDB = new MarketDB();
		MarketDB.Init();

		Logger.LogInfo("Initializing RatEye...");
		SetupRatEye();

		Logger.LogInfo("Setting default item...");
		var inspection = RatEyeEngine.NewInspection(new Bitmap(50, 50));
		CurrentItemScan = new ItemNameScan(inspection);

		Logger.LogInfo("Initializing hotkey manager...");
		HotkeyManager = new HotkeyManager();

		Logger.LogInfo("Setting up data update routines...");
		_marketDBRefreshTimer = new Timer(RefreshMarketDB, null, RatConfig.MarketDBRefreshTime, Timeout.Infinite);
		_tarkovTrackerDBRefreshTimer = new Timer(RefreshTarkovTrackerDB, null, RatConfig.Tracking.TarkovTracker.RefreshTime, Timeout.Infinite);
		_scanRefreshTimer = new Timer(RefreshOverlay);
		//_nameScanTimer = new Timer(NameScanContinuous, null, 500, 1000);

		Logger.LogInfo("Ready!");
	}

	private void CheckForUpdates()
	{
		var mostRecentVersion = ApiManager.GetResource(ApiManager.ResourceType.ClientVersion);
		if (RatConfig.Version == mostRecentVersion) return;
		Logger.LogInfo("A new version is available: " + mostRecentVersion);

		var message = "Version " + mostRecentVersion + " is available!\n";
		message += "You are using: " + RatConfig.Version + "\n\n";
		message += "Do you want to install it now?";
		var result = MessageBox.Show(message, "Rat Scanner Updater", MessageBoxButton.YesNo);
		if (result == MessageBoxResult.Yes) UpdateRatScanner();
	}

	private void UpdateRatScanner()
	{
		if (!File.Exists(RatConfig.Paths.Updater))
		{
			Logger.LogWarning(RatConfig.Paths.Updater + " could not be found!");
			try
			{
				var updaterLink = ApiManager.GetResource(ApiManager.ResourceType.UpdaterLink);
				ApiManager.DownloadFile(updaterLink, RatConfig.Paths.Updater);
				throw new AbandonedMutexException();
			}
			catch (Exception e)
			{
				Logger.LogError("Unable to download updater, please update manually.", e);
				return;
			}
		}

		var startInfo = new ProcessStartInfo(RatConfig.Paths.Updater);
		startInfo.UseShellExecute = true;
		startInfo.ArgumentList.Add("--start");
		startInfo.ArgumentList.Add("--update");
		Process.Start(startInfo);
		Environment.Exit(0);
	}

	private void CheckForItemDataUpdates()
	{
		var mostRecentVersion = ApiManager.GetResource(ApiManager.ResourceType.ItemDataBundleVersion);
		if (!File.Exists(RatConfig.Paths.ItemData) || mostRecentVersion != RatConfig.ItemDataBundleVersion)
		{
			Logger.LogInfo("A new item data bundle is available: " + mostRecentVersion);

			// Download and extract new item data
			var itemDataLink = ApiManager.GetResource(ApiManager.ResourceType.ItemDataBundleLink);
			ApiManager.DownloadFile(itemDataLink, "itemData.zip");
			System.IO.Compression.ZipFile.ExtractToDirectory("itemData.zip", RatConfig.Paths.Data, true);
			File.Delete("itemData.zip");

			// Save the current version number to the config
			RatConfig.ItemDataBundleVersion = mostRecentVersion;
			RatConfig.SaveConfig();

			Logger.LogInfo("Successfully updated to the latest item data bundle");
		}
	}

	internal void SetupRatEye()
	{
		Config.LogDebug = RatConfig.LogDebug;
		Config.Path.LogFile = "RatEyeLog.txt";
		Config.Path.TesseractLibSearchPath = AppDomain.CurrentDomain.BaseDirectory;

		RatEyeConfig = new Config()
		{
			PathConfig = new Config.Path()
			{
				TrainedData = RatConfig.Paths.TrainedData,
				StaticIcons = RatConfig.Paths.StaticIcon,
				StaticCorrelationData = RatConfig.Paths.StaticCorrelation,
				ItemLocales = RatConfig.Paths.Locales,
				ItemData = RatConfig.Paths.ItemData,
			},
			ProcessingConfig = new Config.Processing()
			{
				Scale = Config.Processing.Resolution2Scale(RatConfig.ScreenWidth, RatConfig.ScreenHeight),
				Language = RatConfig.NameScan.Language,
				IconConfig = new Config.Processing.Icon()
				{
					UseStaticIcons = true,
					ScanRotatedIcons = RatConfig.IconScan.ScanRotatedIcons,
				},
				InventoryConfig = new Config.Processing.Inventory()
				{
					OptimizeHighlighted = true,
					MaxGridColor = Color.FromArgb(89, 100, 100),
				},
				InspectionConfig = new Config.Processing.Inspection()
				{
					MarkerThreshold = 0.9f,
					EnableContainers = false,
				},
			},
		};

		RatEyeEngine = new RatEyeEngine(RatEyeConfig);
	}

	/// <summary>
	/// Perform a icon scan at the given position
	/// </summary>
	/// <param name="position">Position on the screen at which to perform the scan</param>
	/// <returns><see langword="true"/> if a item was scanned successfully</returns>
	internal void IconScan(Vector2 position)
	{
		Logger.LogDebug("Icon scanning at: " + position);
		lock (IconScanLock)
		{
			var x = position.X - RatConfig.IconScan.ScanWidth / 2;
			var y = position.Y - RatConfig.IconScan.ScanHeight / 2;

			var screenshotPosition = new Vector2(x, y);
			var size = new Size(RatConfig.IconScan.ScanWidth, RatConfig.IconScan.ScanHeight);
			var screenshot = GetScreenshot(screenshotPosition, size);

			// Scan the item
			var inventory = RatEyeEngine.NewInventory(screenshot);
			var icon = inventory.LocateIcon();

			if (icon?.DetectionConfidence <= 0 || icon?.Item == null) return;

			var toolTipPosition = position;
			toolTipPosition += icon.Position + icon.ItemPosition;
			toolTipPosition -= new Vector2(RatConfig.IconScan.ScanWidth, RatConfig.IconScan.ScanHeight) / 2;

			CurrentItemScan = new ItemIconScan(icon, toolTipPosition, RatConfig.ToolTip.Duration);
			SetOverlayRefresh();
		}
	}

	/// <summary>
	/// Perform a name scan at the give position
	/// </summary>
	/// <param name="position">Position on the screen at which to perform the scan</param>
	/// <returns></returns>
	internal void NameScan(Vector2 position)
	{
		Logger.LogDebug("Name scanning at: " + position);
		lock (NameScanLock)
		{
			// Wait for game ui to update the click
			Thread.Sleep(50);

			// Get raw screenshot which includes the icon and text
			var markerScanSize = RatConfig.NameScan.MarkerScanSize;
			var screenshotPosX = position.X - markerScanSize / 2;
			var screenshotPosY = position.Y - markerScanSize / 2;
			var sizeWidth = markerScanSize + RatConfig.NameScan.TextWidth;
			var sizeHeight = markerScanSize;
			var screenshot = GetScreenshot(new Vector2(screenshotPosX, screenshotPosY), new Size(sizeWidth, sizeHeight));

			// Scan the item
			var inspection = RatEyeEngine.NewInspection(screenshot);

			if (!inspection.ContainsMarker || inspection.Item == null) return;

			var scanSize = RatConfig.NameScan.MarkerScanSize;
			var scale = RatEyeConfig.ProcessingConfig.Scale;
			var toolTipPosition = inspection.MarkerPosition;
			toolTipPosition += position - new Vector2(scanSize, scanSize) / 2;
			toolTipPosition += new Vector2(0, (int)(19f * scale));

			CurrentItemScan = new ItemNameScan(
				inspection,
				toolTipPosition,
				RatConfig.ToolTip.Duration);

			SetOverlayRefresh();
		}
	}

	internal void NameScanContinuous(object? o = null)
	{
		lock (NameScanLock)
		{
			var mousePosition = UserActivityHelper.GetMousePosition();
			var bounds = Screen.AllScreens.Where(screen => screen.Bounds.Contains(mousePosition)).First().Bounds;

			var position = new Vector2(bounds.X, bounds.Y);
			var screenshot = GetScreenshot(position, bounds.Size);

			// Scan the item
			var inspection = RatEyeEngine.NewInspection(screenshot);

			if (!inspection.ContainsMarker || inspection.Item == null) return;

			var scale = RatEyeConfig.ProcessingConfig.Scale;
			var toolTipPosition = inspection.MarkerPosition;
			toolTipPosition += position;
			toolTipPosition += new Vector2(0, (int)(19f * scale));

			CurrentItemScan = new ItemNameScan(
				inspection,
				toolTipPosition,
				RatConfig.ToolTip.Duration);

			SetOverlayRefresh();
		}
	}

	// Returns the ruff screenshot
	private Bitmap GetScreenshot(Vector2 vector2, Size size)
	{
		var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

		try
		{
			using var gfx = Graphics.FromImage(bmp);
			gfx.CopyFromScreen(vector2.X, vector2.Y, 0, 0, size, CopyPixelOperation.SourceCopy);
		}
		catch (Exception e)
		{
			Logger.LogWarning("Unable to capture screenshot", e);
		}

		return bmp;
	}

	private void SetOverlayRefresh()
	{
		_scanRefreshTimer.Change(RatConfig.ToolTip.Duration, Timeout.Infinite);
	}

	private void RefreshOverlay(object? o = null)
	{
		// Overlay will react to event
		OnPropertyChanged();
	}

	private void RefreshMarketDB(object? o = null)
	{
		Logger.LogInfo("Refreshing Market DB...");
		MarketDB.Init();
		_marketDBRefreshTimer.Change(RatConfig.MarketDBRefreshTime, Timeout.Infinite);
	}

	private void RefreshTarkovTrackerDB(object? o = null)
	{
		Logger.LogInfo("Refreshing TarkovTracker DB...");
		TarkovTrackerDB.Init();
		_tarkovTrackerDBRefreshTimer.Change(RatConfig.Tracking.TarkovTracker.RefreshTime, Timeout.Infinite);
	}

	protected virtual void OnPropertyChanged(string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
