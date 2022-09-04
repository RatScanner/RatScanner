using RatScanner.Controls;
using System;
using System.Threading;
using System.Windows.Input;
using static RatScanner.RatConfig;

namespace RatScanner;

internal class HotkeyManager
{
	private long _lastMouseClick = 0;

	internal ActiveHotkey NameScanHotkey;
	internal ActiveHotkey IconScanHotkey;

	internal HotkeyManager()
	{
		UserActivityHelper.Start(true, true);
		RegisterHotkeys();
	}

	~HotkeyManager()
	{
		UnregisterHotkeys();
		UserActivityHelper.Stop(true, true, false);
	}

	/// <summary>
	/// Register hotkeys so the event handlers receive hotkey presses
	/// </summary>
	/// <remarks>
	/// Called by the constructor
	/// </remarks>
	internal void RegisterHotkeys()
	{
		// Unregister hotkeys to prevent multiple listeners for the same hotkey
		UnregisterHotkeys();

		var nameScanHotkey = new Hotkey(null, new[] { MouseButton.Left });
		NameScanHotkey = new ActiveHotkey(nameScanHotkey, OnNameScanHotkey, ref NameScan.Enable);
		IconScanHotkey = new ActiveHotkey(IconScan.Hotkey, OnIconScanHotkey, ref IconScan.Enable);
	}

	/// <summary>
	/// Unregister hotkeys
	/// </summary>
	internal void UnregisterHotkeys()
	{
		NameScanHotkey?.Dispose();
		IconScanHotkey?.Dispose();
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

	private static void Wrap(Action action)
	{
		try
		{
			action();
		}
		catch (Exception e)
		{
			Logger.LogError(e.Message, e);
		}
	}

	private void OnNameScanHotkey(object sender, KeyUpEventArgs e)
	{
		Wrap(() =>
		{
			RatScannerMain.Instance.NameScan(UserActivityHelper.GetMousePosition());
			if (_lastMouseClick + 500 < DateTimeOffset.Now.ToUnixTimeMilliseconds() && NameScan.EnableAuto)
			{
				Thread.Sleep(200);	// wait for double click and ui
				RatScannerMain.Instance.NameScanScreen();
				_lastMouseClick = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			}
		});
	}

	private void OnIconScanHotkey(object sender, KeyUpEventArgs e)
	{
		Wrap(() => RatScannerMain.Instance.IconScan(UserActivityHelper.GetMousePosition()));
	}
}
