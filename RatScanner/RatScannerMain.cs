﻿using RatEye;
using RatScanner.View;
using RatTracking;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
using RatScanner.FetchModels;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using RatLib;

namespace RatScanner;

public class RatScannerMain : INotifyPropertyChanged
{
	private static RatScannerMain _instance = null;
	internal static RatScannerMain Instance => _instance ??= new RatScannerMain();

	internal readonly HotkeyManager HotkeyManager;

	private readonly BlazorOverlay _blazorOverlay;

	private Timer _marketDBRefreshTimer;
	private Timer _tarkovTrackerDBRefreshTimer;
	private Timer _scanRefreshTimer;

	// Check for game screen changes every 10 seconds
	private int _gameScreenRefreshInterval = 10000;
	private Timer _gameScreenRefreshTimer;

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

	internal MarketDB MarketDB = new();
	internal ProgressDB ProgressDB = new();
	public TarkovTrackerDB TarkovTrackerDB;

	internal RatEyeEngine RatEyeEngine;

	public event PropertyChangedEventHandler PropertyChanged;

	internal ItemQueue ItemScans = new();

	public ScreenScale? GameScreenScale => ScreenInfo.GameWindowScreenScale();

	public RatScannerMain()
	{
		_instance = this;

		// Remove old log
		Logger.Clear();

		Logger.LogInfo("----- RatScanner " + RatConfig.Version + " -----");
		Logger.LogInfo("Starting RatScanner...");

		Logger.LogInfo("Loading config...");
		RatConfig.LoadConfig();

		Logger.LogInfo("Setting temporary default item...");
		ItemScans.Enqueue(new DefaultItemScan(true));

		Logger.LogInfo("Checking for item data updates...");
		CheckForItemDataUpdates();

		Logger.LogInfo("Initializing tarkov tracker database");
		TarkovTrackerDB = new TarkovTrackerDB();

		Logger.LogInfo("Initializing hotkey manager...");
		HotkeyManager = new HotkeyManager();
		HotkeyManager.UnregisterHotkeys();

		Logger.LogInfo("UI Ready!");

		new Thread(() =>
		{
			Thread.Sleep(1000);
			Logger.LogInfo("Checking for updates...");
			CheckForUpdates();

			// Grab quest and hideout requirements from tarkovdata
			Logger.LogInfo("Loading progress data...");
			ProgressDB.Init();

			Logger.LogInfo("Loading price data...");
			MarketDB.Init();

			Logger.LogInfo("Loading TarkovTracker data...");
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

			Logger.LogInfo("Initializing RatEye...");
			SetupRatEye();

			Logger.LogInfo("Setting up timer routines...");
			_marketDBRefreshTimer = new Timer(RefreshMarketDB, null, RatConfig.MarketDBRefreshTime, Timeout.Infinite);
			_tarkovTrackerDBRefreshTimer = new Timer(RefreshTarkovTrackerDB, null, RatConfig.Tracking.TarkovTracker.RefreshTime, Timeout.Infinite);
			_scanRefreshTimer = new Timer(RefreshOverlay, null, 1000, 100);
			_gameScreenRefreshTimer = new Timer(CheckGameScreen, null, _gameScreenRefreshInterval, Timeout.Infinite);

			Logger.LogInfo("Setting default item...");
			var itemScan = new DefaultItemScan(false);
			itemScan.BestTraderPrice = itemScan.MatchedItem.GetMaxTraderPrice();
			ItemScans.Enqueue(itemScan);

			Logger.LogInfo("Enabling hotkeys...");
			HotkeyManager.RegisterHotkeys();

			Logger.LogInfo("Ready!");
		}).Start();
	}

	// Check if RatEye needs to be updated to the latest ScreenScale data
	private void CheckGameScreen(object? o = null)
	{
		ScreenScale? gameScale = GameScreenScale;
		if (
			gameScale != null &&
			RatEyeEngine != null &&
			Config.Processing.Resolution2Scale(gameScale.bounds.Width, gameScale.bounds.Height) != RatEyeEngine.Config.ProcessingConfig.Scale
		)
		{
			// We have a new ScreenScale, update RatEye
			// RatEye config will use the game's actual size, but we want to trigger based upon any scaling changes as well
			SetupRatEye();
		}
		_gameScreenRefreshTimer.Change(_gameScreenRefreshInterval, Timeout.Infinite);
	}

	private void CheckForUpdates()
	{
		var mostRecentVersion = ApiManager.GetResource(ApiManager.ResourceType.ClientVersion);
		if (RatConfig.Version == mostRecentVersion) return;
		Logger.LogInfo("A new version is available: " + mostRecentVersion);

		var forceVersions = ApiManager.GetResource(ApiManager.ResourceType.ClientForceUpdateVersions);
		if (forceVersions.Contains($"[{RatConfig.Version}]"))
		{
			UpdateRatScanner();
			return;
		}

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
		RatEyeEngine = new RatEyeEngine(GetRatEyeConfig());
	}

	private RatEye.Config GetRatEyeConfig(bool highlighted = true)
	{
		// Default to our primary screen, then try to set it to game window if we can.
		int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
		int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
		try
		{
			ScreenScale? gameScale = ScreenInfo.GameWindowScreenScale();
			if (gameScale != null)
			{
				System.Drawing.Rectangle gameRect = GameWindowLocator.GetWindowLocation();
				// Set ScreenWidth and ScreenHeight to the dimensions of the gameRect rectangle
				ScreenWidth = gameRect.Width;
				ScreenHeight = gameRect.Height;
			}
		}
		catch {
			// We must not have been able to find the game window, use the primary screen's dimensions as placeholder
		}
		return new Config()
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
				Scale = Config.Processing.Resolution2Scale(ScreenWidth, ScreenHeight),
				Language = RatConfig.NameScan.Language,
				IconConfig = new Config.Processing.Icon()
				{
					UseStaticIcons = true,
					ScanRotatedIcons = RatConfig.IconScan.ScanRotatedIcons,
				},
				InventoryConfig = new Config.Processing.Inventory()
				{
					OptimizeHighlighted = highlighted,
					MaxGridColor = Color.FromArgb(89, 100, 100),
				},
				InspectionConfig = new Config.Processing.Inspection()
				{
					MarkerThreshold = 0.9f,
				},
			},
		};
	}

	/// <summary>
	/// Perform a name scan at the give position
	/// </summary>
	/// <param name="position">Position on the screen at which to perform the scan</param>
	internal void NameScan(Vector2 position)
	{
		lock (NameScanLock)
		{
			Logger.LogDebug("Name scanning at: " + position);
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

			var tempNameScan = new ItemNameScan(
				inspection,
				toolTipPosition,
				RatConfig.ToolTip.Duration);
			tempNameScan.ImageLink = tempNameScan.MatchedItem.GetMarketItem().ImageLink;
			tempNameScan.IconLink = tempNameScan.MatchedItem.GetMarketItem().IconLink;
			tempNameScan.WikiLink = tempNameScan.MatchedItem.GetMarketItem().WikiLink;
			tempNameScan.TarkovDevLink = $"https://tarkov.dev/item/{tempNameScan.MatchedItem.Id}";
			tempNameScan.Avg24hPrice = tempNameScan.MatchedItem.GetMarketItem().Avg24hPrice;
			tempNameScan.PricePerSlot = tempNameScan.MatchedItem.GetMarketItem().Avg24hPrice / (tempNameScan.MatchedItem.Width * tempNameScan.MatchedItem.Height);
			tempNameScan.TraderName = TraderPrice.GetTraderName(tempNameScan.MatchedItem.GetBestTrader().traderId);
			tempNameScan.BestTraderPrice = tempNameScan.MatchedItem.GetBestTrader().price;

			ItemScans.Enqueue(tempNameScan);

			RefreshOverlay();
		}
	}

	/// <summary>
	/// Perform a name scan over the entire active screen
	/// </summary>
	internal void NameScanScreen(object? _ = null)
	{
		lock (NameScanLock)
		{
			Logger.LogDebug("Name scanning screen");
			var mousePosition = UserActivityHelper.GetMousePosition();
			var bounds = Screen.AllScreens.First(screen => screen.Bounds.Contains(mousePosition)).Bounds;

			var position = new Vector2(bounds.X, bounds.Y);
			var screenshot = GetScreenshot(position, bounds.Size);

			// Scan the item
			var multiInspection = RatEyeEngine.NewMultiInspection(screenshot);

			if (multiInspection.Inspections.Count == 0) return;

			foreach (var inspection in multiInspection.Inspections)
			{
				var scale = RatEyeEngine.Config.ProcessingConfig.Scale;
				var toolTipPosition = inspection.MarkerPosition;
				toolTipPosition += position;
				var marker = RatEyeEngine.Config.ProcessingConfig.InspectionConfig.Marker;
				toolTipPosition += new Vector2(0, (int)(marker.Height * scale));

				var tempNameScan = new ItemNameScan(
						inspection,
						toolTipPosition,
						RatConfig.ToolTip.Duration)
				{
					ImageLink = inspection.Item.GetMarketItem().ImageLink,
					IconLink = inspection.Item.GetMarketItem().IconLink,
					WikiLink = inspection.Item.GetMarketItem().WikiLink,
					TarkovDevLink = $"https://tarkov.dev/item/{inspection.Item.Id}",
					Avg24hPrice = inspection.Item.GetMarketItem().Avg24hPrice,
					PricePerSlot = inspection.Item.GetMarketItem().Avg24hPrice / (inspection.Item.Width * inspection.Item.Height),
					TraderName = TraderPrice.GetTraderName(inspection.Item.GetBestTrader().traderId),
					BestTraderPrice = inspection.Item.GetBestTrader().price,
				};

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
	internal void IconScan(Vector2 position)
	{
		lock (IconScanLock)
		{
			Logger.LogDebug("Icon scanning at: " + position);
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

			var tempIconScan = new ItemIconScan(icon, toolTipPosition, RatConfig.ToolTip.Duration)
			{
				ImageLink = icon.Item.GetMarketItem().ImageLink,
				IconLink = icon.Item.GetMarketItem().IconLink,
				WikiLink = icon.Item.GetMarketItem().WikiLink,
				TarkovDevLink = $"https://tarkov.dev/item/{icon.Item.Id}",
				Avg24hPrice = icon.Item.GetMarketItem().Avg24hPrice,
				PricePerSlot = icon.Item.GetMarketItem().Avg24hPrice / (icon.Item.Width * icon.Item.Height),
				TraderName = TraderPrice.GetTraderName(icon.Item.GetBestTrader().traderId),
				BestTraderPrice = icon.Item.GetBestTrader().price,
			};

			ItemScans.Enqueue(tempIconScan);
			RefreshOverlay();
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

	private void RefreshOverlay(object? o = null)
	{
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
