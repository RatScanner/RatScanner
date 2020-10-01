using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RatScanner.Scan;
using RatScanner.View;
using Size = System.Drawing.Size;

namespace RatScanner
{
	public class RatScannerMain : INotifyPropertyChanged
	{
		private static RatScannerMain _instance = null;
		internal static RatScannerMain Instance => _instance ??= new RatScannerMain();

		private UserActivityHook activityHook;

		private readonly NameScanToolTip _nameScanToolTip;
		private readonly IconScanToolTip _iconScanToolTip;

		private ItemScan _currentItemScan;
		internal bool ScanLock = false;

		internal MarketDB MarketDB;

		internal bool ModifierDown;

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

		internal RatScannerMain()
		{
			_instance = this;

			// Remove old log
			Logger.Clear();

			Logger.LogInfo("----- RatScanner " + RatConfig.Version + " -----");
			Logger.LogInfo("Starting RatScanner...");

			Logger.LogInfo("Loading config...");
			RatConfig.LoadConfig();

			Logger.LogInfo("Loading price data...");
			MarketDB = new MarketDB();
			MarketDB.Init();

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

			Logger.LogInfo("Registering mouse and keyboard hooks...");
			activityHook = new UserActivityHook();
			activityHook.OnMouseActivity += OnMouseEvent;
			activityHook.KeyDown += OnKeyDown;
			activityHook.KeyUp += OnKeyUp;

			Logger.LogInfo("Ready!");

			// Check for new versions
			var mostRecentVersion = ApiManager.GetResource(ApiManager.ResourceType.ClientVersion);
			if (RatConfig.Version != mostRecentVersion)
			{
				var message = "Version " + mostRecentVersion + " is available!\n";
				message += "Get it from GitHub or join our Discord.\n\n";
				message += "You are using: " + RatConfig.Version;
				MessageBox.Show(message, "Rat Scanner Updater");
				Logger.LogInfo("A new version is available: " + mostRecentVersion);
			}
		}

		private void OnMouseEvent(object sender, MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == 0) return;

			try
			{
				if (ModifierDown)
				{
					Task.Run(delegate
					{
						if (RatConfig.IconScan.Enable) IconScan(new Vector2(e.Location));
					});
				}
				else
				{
					Task.Run(delegate
					{
						if (RatConfig.NameScan.Enable) NameScan(new Vector2(e.Location));
					});
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.Message, ex);
			}
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if ((int)e.KeyCode == RatConfig.IconScan.ModifierKeyCode) ModifierDown = true;
		}

		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			if ((int)e.KeyCode == RatConfig.IconScan.ModifierKeyCode) ModifierDown = false;
		}

		private bool IconScan(Vector2 mouseVector2)
		{
			if (ScanLock) return false;
			ScanLock = true;

			_iconScanToolTip.Dispatcher.Invoke(() =>
			{
				IconScanToolTip.ScheduleHide();
				_iconScanToolTip.Hide();    // Hide it instantly
			});

			var x = mouseVector2.X - (RatConfig.IconScan.ScanWidth / 2);
			var y = mouseVector2.Y - (RatConfig.IconScan.ScanHeight / 2);

			var position = new Vector2(x, y);
			var size = new Size(RatConfig.IconScan.ScanWidth, RatConfig.IconScan.ScanHeight);
			var screenshot = GetScreenshot(position, size);

			var itemIconScan = new ItemIconScan(screenshot, mouseVector2);

			if (!itemIconScan.ValidItem)
			{
				ScanLock = false;
				return false;
			}
			CurrentItemScan = itemIconScan;

			ShowToolTip(itemIconScan);

			ScanLock = false;
			return true;
		}

		private bool NameScan(Vector2 mouseVector2)
		{
			if (ScanLock) return false;
			ScanLock = true;

			_nameScanToolTip.Dispatcher.Invoke(() =>
			{
				NameScanToolTip.ScheduleHide();
				_nameScanToolTip.Hide();    // Hide it instantly
			});

			// Wait for game ui to update the click
			Thread.Sleep(50);

			// Get raw screenshot which includes the icon and text
			var markerScanSize = RatConfig.NameScan.MarkerScanSize;
			var positionX = mouseVector2.X - (markerScanSize / 2);
			var positionY = mouseVector2.Y - (markerScanSize / 2);
			var sizeWidth = markerScanSize + RatConfig.NameScan.TextWidth + RatConfig.NameScan.TextHorizontalOffset;
			var sizeHeight = markerScanSize;
			var screenshot = GetScreenshot(new Vector2(positionX, positionY), new Size(sizeWidth, sizeHeight));

			// Scan the item
			var itemNameScan = new ItemNameScan(screenshot, mouseVector2);

			if (!itemNameScan.ValidItem)
			{
				ScanLock = false;
				return false;
			}
			CurrentItemScan = itemNameScan;

			ShowToolTip(itemNameScan);

			ScanLock = false;
			return true;
		}

		// Returns the ruff screenshot
		private Bitmap GetScreenshot(Vector2 vector2, Size size)
		{
			var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

			using (var gfx = Graphics.FromImage(bmp))
			{
				gfx.CopyFromScreen(vector2.X, vector2.Y, 0, 0, size, CopyPixelOperation.SourceCopy);
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

		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
