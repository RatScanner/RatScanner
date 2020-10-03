using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using RatScanner.Scan;
using RatScanner.ViewModel;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for NameScanToolTip.xaml
	/// </summary>
	internal partial class NameScanToolTip : Window
	{
		internal NameScanToolTip()
		{
			InitializeComponent();
			Loaded += ToolTip_Load;
		}

		private static long hideAfter;

		private static ItemScan itemScan;

		private static Vector2 vector2;

		private static bool forceUpdate;

		private readonly Timer _updateTimer = new Timer();

		internal static void ScheduleShow(ItemScan sItem, Vector2 pos, int duration)
		{
			itemScan = sItem;
			vector2 = pos;

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
			Left = vector2.X;
			Top = vector2.Y;

			Show();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			var hwnd = new WindowInteropHelper(this).Handle;
			WindowsServices.SetWindowExTransparent(hwnd);
		}
	}
}
