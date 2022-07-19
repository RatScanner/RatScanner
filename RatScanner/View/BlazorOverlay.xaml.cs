using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Drawing;
using System.Timers;

namespace RatScanner.View;

/// <summary>
/// Interaction logic for BlazorOverlay.xaml
/// </summary>
public partial class BlazorOverlay : Window
{
	private IntPtr overlayHandle;
	public BlazorOverlay(ServiceProvider serviceProvider)
	{
		Resources.Add("services", serviceProvider);

		overlayHandle = new WindowInteropHelper(this).Handle;
		// Set the  overlay window style such that alt+tab does not show it in the switcher
		SetWindowLong(overlayHandle, GWL_EX_STYLE, (GetWindowLong(overlayHandle, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);

		InitializeComponent();
	}

	private void BlazorOverlay_Loaded(object sender, RoutedEventArgs e)
	{
		blazorOverlayWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;

		// Set up the window check timer
		initializeWindowCheckTimer();

		// Finish loading the webview
		blazorOverlayWebView.WebView.NavigationCompleted += WebView_Loaded;
		blazorOverlayWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;

		// Other customizations
		this.IsHitTestVisible = false;
	}
	
	private void initializeWindowCheckTimer()
	{
		// Set up the timer with interval
		windowCheckTimer = new System.Timers.Timer(10000);
		windowCheckTimer.Elapsed += checkWindowEvent;
		windowCheckTimer.AutoReset = true;
		windowCheckTimer.Enabled = true;

		// Trigger the timer immediately at initialization
		checkWindowEvent(null, null);
	}

	private void checkWindowEvent(Object source, ElapsedEventArgs e)
	{
		// Used to find the area of the EFT game client to set up the overlay location and size
		IntPtr hwnd = FindWindow("UnityWndClass", "EscapeFromTarkov");

		// Check if we found the EFT window
		if (hwnd != IntPtr.Zero)
		{
			// Nonzero intptr means we found it
			Rectangle rect;
			GetWindowRect(hwnd, out rect);

			Debug.WriteLine($"Found the EFT Window at {rect.X}, {rect.Y}, {rect.Width}, {rect.Height}");

			// Set the size of the overlay now that we have the rect of EFT
			//SetSize(rect.Left, rect.Top, rect.Width, rect.Height);
			Dispatcher.Invoke(() =>
			{
				this.Left = rect.Left;
				this.Top = rect.Top;
				this.Width = Math.Abs(rect.Right - rect.Left) / scaleFactor;
				this.Height = Math.Abs(rect.Bottom - rect.Top) / scaleFactor;

				// We get the scale factor of the latest screen that we've moved onto
				// So this will take one invocation before it becomes correct if the user either resizes or moves the window
				scaleFactor = System.Windows.PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
			});
		}
		else
		{
			Debug.WriteLine("Did not find the EFT window for the overlay");
		}
	}

	// Used to hide the overlay from the alt+tab switcher
	[DllImport("user32.dll", SetLastError = true)]
	static extern int GetWindowLong(IntPtr hWnd, int nIndex);
	[DllImport("user32.dll")]
	static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
	private const int GWL_EX_STYLE = -20;
	private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

	// Used for setting window position
	[DllImport("user32.dll", SetLastError = true)]
	static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, UInt32 uFlags);

	// Used to find the EscapeFromTarkov window to position the overlay
	[DllImport("user32.dll")]
	private static extern IntPtr FindWindow(string className, string windowName);
	[DllImport("user32.dll")]
	private static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);

	// Repeat timer to check for the EFT window and resize if necessary
	private static System.Timers.Timer windowCheckTimer;

	// Used for display scaling calculations
	private double scaleFactor = 1.0;

	private void WebView_Loaded(object sender, CoreWebView2NavigationCompletedEventArgs e)
	{
		// If we are running in a development/debugger mode, open dev tools to help out
		if (Debugger.IsAttached) blazorOverlayWebView.WebView.CoreWebView2.OpenDevToolsWindow();
	}

	private void CoreWebView_Loaded(object sender, CoreWebView2InitializationCompletedEventArgs e)
	{
		blazorOverlayWebView.WebView.CoreWebView2.SetVirtualHostNameToFolderMapping("local.data", "Data", CoreWebView2HostResourceAccessKind.Allow);
	}
}
