using System;
using System.Windows.Input;
using RatScanner.Controls;
using static RatScanner.RatConfig;

namespace RatScanner
{
	internal class HotkeyManager
	{
		internal ActiveHotkey NameScanHotkey;
		internal ActiveHotkey IconScanHotkey;

		internal HotkeyManager()
		{
			UserActivityHelper.Start(true, true);
			RegisterHotkeys();
		}

		~HotkeyManager()
		{
			UserActivityHelper.Stop(true, true, false);
		}

		/// <summary>
		/// Register the hotkeys so the event handlers receive hotkey presses
		/// </summary>
		/// <remarks>
		/// Called by the constructor
		/// </remarks>
		internal void RegisterHotkeys()
		{
			// Destroy old active hotkeys to prevent duplicated hotkey
			NameScanHotkey?.Dispose();
			IconScanHotkey?.Dispose();

			// Register new hotkeys
			var nameScanHotkey = new Hotkey(null, new[] { MouseButton.Left });
			NameScanHotkey = new ActiveHotkey(nameScanHotkey, OnNameScanHotkey, ref NameScan.Enable);
			IconScanHotkey = new ActiveHotkey(IconScan.Hotkey, OnIconScanHotkey, ref IconScan.Enable);
		}

		private static void Wrap<T>(Func<T> func)
		{
			try
			{
				func();
			}
			catch (Exception e)
			{
				Logger.LogError(e.Message, e);
			}
		}

		private void OnNameScanHotkey(object sender, KeyUpEventArgs e)
		{
			Wrap(() => RatScannerMain.Instance.NameScan(UserActivityHelper.GetMousePosition()));
		}

		private void OnIconScanHotkey(object sender, KeyUpEventArgs e)
		{
			Wrap(() => RatScannerMain.Instance.IconScan(UserActivityHelper.GetMousePosition()));
		}
	}
}
