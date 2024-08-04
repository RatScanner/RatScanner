using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace RatScanner.View;

/// <summary>
/// Interaction logic for BlazorInteractableOverlay.xaml
/// </summary>
public partial class BlazorInteractableOverlay : Window {
	public BlazorInteractableOverlay(ServiceProvider serviceProvider) {
		Resources.Add("services", serviceProvider);

		InitializeComponent();
	}

	private void BlazorInteractableOverlay_Loaded(object? sender, RoutedEventArgs e) {
		blazorInteractableOverlayWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
		SetWindowStyle();
		SetPosition();
		ApplyBlurBehind();
		blazorInteractableOverlayWebView.WebView.NavigationCompleted += WebView_Loaded;
		blazorInteractableOverlayWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
	}

	private void SetPosition() {
		System.Collections.Generic.IEnumerable<Screen> hoveredScreen = Screen.AllScreens.Where(screen => screen.Bounds.Contains(UserActivityHelper.GetMousePosition()));
		Screen? screen = hoveredScreen.FirstOrDefault() ?? Screen.PrimaryScreen;
		System.Drawing.Rectangle b = screen.Bounds;
		nint handle = new WindowInteropHelper(this).Handle;
		NativeMethods.SetWindowPos(handle, 0, b.Left, b.Top, b.Right - b.Left, b.Bottom - b.Top, 0);
	}

	private void ApplyBlurBehind() {
		WindowBlurEffect.AccentState accent = WindowBlurEffect.AccentState.ACCENT_DISABLED;
		if (RatConfig.Overlay.Search.BlurBehind) accent = WindowBlurEffect.AccentState.ACCENT_ENABLE_BLURBEHIND;
		WindowBlurEffect.SetBlur(this, accent);
	}

	async internal void ShowOverlay() {
		ApplyBlurBehind();
		SetPosition();
		Show();
		await blazorInteractableOverlayWebView.WebView.EnsureCoreWebView2Async();
		await blazorInteractableOverlayWebView.WebView.ExecuteScriptAsync("ShowOverlay()");
	}

	internal void HideOverlay() {
		Hide();
	}

	private void SetWindowStyle() {
		const int gwlExStyle = -20; // GWL_EXSTYLE
		const uint wsExToolWindow = 0x00000080; // WS_EX_TOOLWINDOW

		nint handle = new WindowInteropHelper(this).Handle;
		NativeMethods.SetWindowLongPtr(handle, gwlExStyle, NativeMethods.GetWindowLongPtr(handle, gwlExStyle) | (nint)wsExToolWindow);
	}

	private void WebView_Loaded(object? sender, CoreWebView2NavigationCompletedEventArgs e) {
		// if we are running in a debug mode, open dev tools to help out
		if (Debugger.IsAttached) blazorInteractableOverlayWebView.WebView.CoreWebView2.OpenDevToolsWindow();
	}

	private void CoreWebView_Loaded(object? sender, CoreWebView2InitializationCompletedEventArgs e) {
		blazorInteractableOverlayWebView.WebView.CoreWebView2.SetVirtualHostNameToFolderMapping("local.data", "Data", CoreWebView2HostResourceAccessKind.Allow);
		blazorInteractableOverlayWebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
		blazorInteractableOverlayWebView.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
	}

	private static class NativeMethods {
		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
		public static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
		public static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	}
}
