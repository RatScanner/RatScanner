using RatScanner.View;
using RatScanner.TarkovDev.Json;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows;
using static RatScanner.RatConfig;
using OverlayC = RatScanner.RatConfig.Overlay;

namespace RatScanner;

internal class HotkeyManager {
	private long _last_mouse_click = 0;

	internal ActiveHotkey NameScanHotkey;
	internal ActiveHotkey IconScanHotkey;
	internal ActiveHotkey OpenInteractableOverlayHotkey;
	internal ActiveHotkey EscapeKeyHotkey;
	internal ActiveHotkey OpenWikiHotkey;
	internal ActiveHotkey OpenTarkovDevHotkey;

	/// <summary>
	/// Raised when the Escape hotkey fires, letting an open view consume the key
	/// before it falls through to closing the interactive overlay.
	///
	/// A subscriber returns <see langword="true"/> if it handled the key. The
	/// first one to do so wins and no later subscriber is called, so whichever
	/// view is on top of the others gets the key. With nobody handling it the
	/// overlay closes, which is what Escape has always done.
	/// </summary>
	internal event Func<bool>? EscapePressed;

	internal HotkeyManager() {
		UserActivityHelper.Start(true, true);
		RegisterHotkeys();
	}

	~HotkeyManager() {
		UnregisterHotkeys();
		UserActivityHelper.Stop(true, true, false);
	}

	/// <summary>
	/// Register hotkeys so the event handlers receive hotkey presses
	/// </summary>
	/// <remarks>
	/// Called by the constructor
	/// </remarks>
	[MemberNotNull(
		nameof(NameScanHotkey),
		nameof(IconScanHotkey),
		nameof(OpenInteractableOverlayHotkey),
		nameof(EscapeKeyHotkey),
		nameof(OpenWikiHotkey),
		nameof(OpenTarkovDevHotkey))
	]
	internal void RegisterHotkeys() {
		// Unregister hotkeys to prevent multiple listeners for the same hotkey
		UnregisterHotkeys();

		NameScanHotkey = new ActiveHotkey(NameScan.Hotkey, OnNameScanHotkey, ref NameScan.Enable);
		IconScanHotkey = new ActiveHotkey(IconScan.Hotkey, OnIconScanHotkey, ref IconScan.Enable);
		OpenInteractableOverlayHotkey = new ActiveHotkey(OverlayC.Search.Hotkey, OnOpenInteractableOverlayHotkey, ref OverlayC.Search.Enable);
		EscapeKeyHotkey = new ActiveHotkey(OverlayC.Search.CloseHotkey, OnEscapeKey);
		OpenWikiHotkey = new ActiveHotkey(Hotkeys.OpenWiki, OnOpenWikiHotkey);
		OpenTarkovDevHotkey = new ActiveHotkey(Hotkeys.OpenTarkovDev, OnOpenTarkovDevHotkey);
	}

	/// <summary>
	/// Unregister hotkeys
	/// </summary>
	internal void UnregisterHotkeys() {
		NameScanHotkey?.Dispose();
		IconScanHotkey?.Dispose();
		OpenInteractableOverlayHotkey?.Dispose();
		EscapeKeyHotkey?.Dispose();
		OpenWikiHotkey?.Dispose();
		OpenTarkovDevHotkey?.Dispose();
	}

	private static void Wrap(Action action) {
		try {
			action();
		} catch (Exception e) {
			Logger.LogError(e.Message, e);
		}
	}

	private void OnNameScanHotkey(object? sender, KeyUpEventArgs e) {
		Wrap(() => {
			RatScannerMain.Instance.NameScan(UserActivityHelper.GetMousePosition());
			if (_last_mouse_click + 500 < DateTimeOffset.Now.ToUnixTimeMilliseconds() && NameScan.EnableAuto) {
				Thread.Sleep(200);  // wait for double click and ui
				RatScannerMain.Instance.NameScanScreen();
				_last_mouse_click = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			}
		});
	}

	private void OnIconScanHotkey(object? sender, KeyUpEventArgs e) {
		Wrap(() => RatScannerMain.Instance.IconScan(UserActivityHelper.GetMousePosition()));
	}

	private void OnOpenInteractableOverlayHotkey(object? sender, KeyUpEventArgs e) {
		Wrap(() => Application.Current.Dispatcher.Invoke(() => Wrap(() => BlazorUI.BlazorInteractableOverlay.ShowOverlay())));
	}

	/// <summary>
	/// Escape. Offers the key to <see cref="EscapePressed"/> first and only hides
	/// the overlay if nothing wanted it, so an open map can take the key and
	/// close itself instead of the whole overlay vanishing underneath it.
	/// </summary>
	private void OnEscapeKey(object? sender, KeyUpEventArgs e) {
		Wrap(() => Application.Current.Dispatcher.Invoke(() => Wrap(() => {
			// Snapshot before invoking: a handler may unsubscribe, and this must
			// not then re-enter a handler that has already gone away.
			var handlers = EscapePressed?.GetInvocationList();
			if (handlers != null) {
				foreach (var handler in handlers) {
					if ((bool)((Func<bool>)handler)()) return;
				}
			}

			BlazorUI.BlazorInteractableOverlay.HideOverlay();
		})));
	}

	private void OnOpenWikiHotkey(object? sender, KeyUpEventArgs e) {
		Wrap(() => {
			var item = LastScannedItem();
			if (item is null) return;
			var link = item.WikiLink;
			if (string.IsNullOrEmpty(link) || link.Length <= 3) {
				link = $"https://escapefromtarkov.gamepedia.com/{Uri.EscapeDataString(item.Name.Replace(" ", "_"))}";
			}
			OpenURL(link);
		});
	}

	private void OnOpenTarkovDevHotkey(object? sender, KeyUpEventArgs e) {
		Wrap(() => {
			var item = LastScannedItem();
			OpenURL(item?.Link);
		});
	}

	/// <summary>
	/// The most recently scanned item, or null when nothing has been scanned yet.
	/// </summary>
	private static Item? LastScannedItem() {
		var scans = RatScannerMain.Instance.ItemScans;
		for (var i = scans.Count - 1; i >= 0; i--) {
			var item = scans.ElementAtOrDefault(i)?.Item;
			if (item is not null) return item;
		}
		return null;
	}

	private static void OpenURL(string? url) {
		if (string.IsNullOrEmpty(url)) return;
		_ = Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
	}
}
