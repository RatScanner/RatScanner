using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using RatScanner.Models;
using RatScanner.Scan;
using RatScanner.View;
using RatStash;
using Size = System.Drawing.Size;
using Timer = System.Threading.Timer;

namespace RatScanner
{
	public class RatScannerMain : INotifyPropertyChanged
	{
		private static RatScannerMain _instance = null;
		internal static RatScannerMain Instance => _instance ??= new RatScannerMain();

		internal readonly HotkeyManager HotkeyManager;

		private readonly NameScanToolTip _nameScanToolTip;
		private readonly IconScanToolTip _iconScanToolTip;

		private ItemScan _currentItemScan;

		private Timer _marketDBRefreshTimer;
		private Timer _tarkovTrackerDBRefreshTimer;


		/// <summary>
		/// Lock for name scanning
		/// </summary>
		/// <remarks>
		/// Lock order: 0
		/// </remarks>
		internal static object NameScanLock = new object();

		/// <summary>
		/// Lock for icon scanning
		/// </summary>
		/// <remarks>
		/// Lock order: 1
		/// </remarks>
		internal static object IconScanLock = new object();

		internal MarketDB MarketDB;
		internal ProgressDB ProgressDB;
		internal TarkovTrackerDB TarkovTrackerDB;
		internal Database ItemDB;
		internal List<WishlistModel> WishlistDB;

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

		private RatScannerMain()
		{
			_instance = this;

			// Remove old log
			Logger.Clear();

			Logger.LogInfo("----- RatScanner " + RatConfig.Version + " -----");
			Logger.LogInfo("Starting RatScanner...");

			Logger.LogInfo("Loading config...");
			RatConfig.LoadConfig();

			Logger.LogInfo("Loading item data...");
			LoadItemDatabase();

			// Check for TarkovTracker data
			TarkovTrackerDB = new TarkovTrackerDB();
			if (RatConfig.Tracking.TarkovTracker.Enable)
			{
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

			Logger.LogInfo("Loading wishlist data...");
			LoadWishlist();

			Logger.LogInfo("Setting default item...");
			CurrentItemScan = new ItemNameScan();

			// Init item scan types
			Logger.LogInfo("Initializing name scan...");
			ItemNameScan.Init();

			Logger.LogInfo("Initializing icon scan...");
			ItemIconScan.Init();

			// Prewarm tool tips
			Logger.LogInfo("Prewarming name tool tip...");
			_nameScanToolTip = new NameScanToolTip();
			_nameScanToolTip.Show();

			Logger.LogInfo("Prewarming icon tool tip...");
			_iconScanToolTip = new IconScanToolTip();
			_iconScanToolTip.Show();

			Logger.LogInfo("Initializing hotkey manager...");
			HotkeyManager = new HotkeyManager();

			Logger.LogInfo("Checking for new updates...");
			CheckForUpdates();

			Logger.LogInfo("Setting up data update routines...");
			_marketDBRefreshTimer = new Timer(RefreshMarketDB, null, RatConfig.MarketDBRefreshTime, Timeout.Infinite);
			_tarkovTrackerDBRefreshTimer = new Timer(RefreshTarkovTrackerDB, null, RatConfig.Tracking.TarkovTracker.RefreshTime, Timeout.Infinite);

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
			if (!File.Exists(RatConfig.Paths.Updater)) Logger.LogError(RatConfig.Paths.Updater + " could not be found! Please update manually.");
			var startInfo = new ProcessStartInfo(RatConfig.Paths.Updater);
			startInfo.UseShellExecute = true;
			startInfo.ArgumentList.Add("--start");
			startInfo.ArgumentList.Add("--update");
			Process.Start(startInfo);
			Environment.Exit(0);
		}

		internal async Task SaveWishlist()
		{
			await File.WriteAllTextAsync(RatConfig.Paths.Wishlist, JsonConvert.SerializeObject(WishlistDB));
		}

		private void LoadWishlist()
		{
			WishlistDB = new List<WishlistModel>();

			if (File.Exists(RatConfig.Paths.Wishlist))
			{
				WishlistDB = JsonConvert.DeserializeObject<List<WishlistModel>>(File.ReadAllText(RatConfig.Paths.Wishlist));
			}
		}

		private void LoadItemDatabase()
		{
			var mostRecentVersion = ApiManager.GetResource(ApiManager.ResourceType.ItemDataVersion);
			if (mostRecentVersion != RatConfig.ItemDataVersion)
			{
				Logger.LogInfo("A new item data version is available: " + mostRecentVersion);
				var itemDataLink = ApiManager.GetResource(ApiManager.ResourceType.ItemDataLink);
				ApiManager.DownloadFile(itemDataLink, RatConfig.Paths.ItemData);
				RatConfig.ItemDataVersion = mostRecentVersion;
				RatConfig.SaveConfig();
			}

			var itemDB = Database.FromFile(RatConfig.Paths.ItemData);
			ItemDB = itemDB.Filter(item => !item.QuestItem);
		}

		/// <summary>
		/// Perform a icon scan at the given position
		/// </summary>
		/// <param name="position">Position on the screen at which to perform the scan</param>
		/// <returns><see langword="true"/> if a item was scanned successfully</returns>
		internal bool IconScan(Vector2 position)
		{
			Logger.LogDebug("Icon scanning at: " + position);
			lock (IconScanLock)
			{
				_iconScanToolTip.Dispatcher.Invoke(() =>
				{
					IconScanToolTip.ScheduleHide();
					_iconScanToolTip.Hide(); // Hide it instantly
				});

				var x = position.X - RatConfig.IconScan.ScanWidth / 2;
				var y = position.Y - RatConfig.IconScan.ScanHeight / 2;

				var screenshotPosition = new Vector2(x, y);
				var size = new Size(RatConfig.IconScan.ScanWidth, RatConfig.IconScan.ScanHeight);
				var screenshot = GetScreenshot(screenshotPosition, size);

				var itemIconScan = new ItemIconScan(screenshot, position);

				if (!itemIconScan.ValidItem) return false;

				CurrentItemScan = itemIconScan;

				ShowToolTip(itemIconScan);
			}

			return true;
		}

		/// <summary>
		/// Perform a name scan at the give position
		/// </summary>
		/// <param name="position">Position on the screen at which to perform the scan</param>
		/// <returns></returns>
		internal bool NameScan(Vector2 position)
		{
			Logger.LogDebug("Name scanning at: " + position);
			lock (NameScanLock)
			{
				_nameScanToolTip.Dispatcher.Invoke(() =>
				{
					NameScanToolTip.ScheduleHide();
					_nameScanToolTip.Hide(); // Hide it instantly
				});

				// Wait for game ui to update the click
				Thread.Sleep(50);

				// Get raw screenshot which includes the icon and text
				var markerScanSize = RatConfig.NameScan.MarkerScanSize;
				var screenshotPosX = position.X - markerScanSize / 2;
				var screenshotPosY = position.Y - markerScanSize / 2;
				var sizeWidth = markerScanSize + RatConfig.NameScan.TextWidth + RatConfig.NameScan.TextHorizontalOffset;
				var sizeHeight = markerScanSize;
				var screenshot = GetScreenshot(new Vector2(screenshotPosX, screenshotPosY), new Size(sizeWidth, sizeHeight));

				// Scan the item
				var itemNameScan = new ItemNameScan(screenshot, position);

				if (!itemNameScan.ValidItem) return false;
				CurrentItemScan = itemNameScan;

				ShowToolTip(itemNameScan);
			}

			return true;
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

		// Display the item information in a ToolTip
		private void ShowToolTip(ItemScan itemScan)
		{
			var pos = itemScan.GetToolTipPosition();
			if (pos == null) return;

			if (itemScan is ItemNameScan) NameScanToolTip.ScheduleShow(itemScan, pos, RatConfig.ToolTip.Duration);
			if (itemScan is ItemIconScan) IconScanToolTip.ScheduleShow(itemScan, pos, RatConfig.ToolTip.Duration);
		}

		private void RefreshMarketDB(object? o)
		{
			Logger.LogInfo("Refreshing Market DB...");
			MarketDB.Init();
			_marketDBRefreshTimer.Change(RatConfig.MarketDBRefreshTime, Timeout.Infinite);
		}

		private void RefreshTarkovTrackerDB(object? o)
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
}
