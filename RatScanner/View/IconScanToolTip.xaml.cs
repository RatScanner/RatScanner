using RatLib;
using RatLib.Scan;
using RatScanner.ViewModel;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Point = System.Windows.Point;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for IconScanToolTip.xaml
	/// </summary>
	internal partial class IconScanToolTip : Window
	{
		internal IconScanToolTip()
		{
			InitializeComponent();
			Loaded += ToolTip_Load;
		}

		private static long hideAfter;

		private static ItemScan itemScan;

		private static Vector2 position;

		private static bool forceUpdate;

		private readonly Timer _updateTimer = new Timer();

		internal static void ScheduleShow(ItemScan sItem, Vector2 pos, int duration)
		{
			itemScan = sItem;
			position = pos;

			var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			hideAfter = timestamp + duration;
			forceUpdate = true;
		}

		internal static void ScheduleHide()
		{
			hideAfter = 0;
			forceUpdate = true;
		}

		private void ToolTip_Load(object sender, EventArgs e)
		{
			_updateTimer.Interval = 1;
			_updateTimer.Tick += Update_Tick;
			_updateTimer.Start();
		}

		private void Update_Tick(object sender, EventArgs e)
		{
			if (DateTimeOffset.Now.ToUnixTimeMilliseconds() > hideAfter)
			{
				if (IsVisible) Hide();
				return;
			}

			if (IsVisible && !forceUpdate) return;
			forceUpdate = false;

			DataContext = new ToolTipVM(itemScan);
			var devicePosition = FromDevicePixels(position);
			Left = devicePosition.X;
			Top = devicePosition.Y;

			Show();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			var hwnd = new WindowInteropHelper(this).Handle;
			WindowsServices.SetWindowExTransparent(hwnd);
		}

		private Vector2 FromDevicePixels(Vector2 size)
		{
			var source = PresentationSource.FromVisual(this);
			var transformMatrix = source.CompositionTarget.TransformFromDevice;

			var pixels = transformMatrix.Transform(new Point(size.X, size.Y));
			return new Vector2((int)pixels.X, (int)pixels.Y);
		}
	}
}
