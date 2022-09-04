using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Timers;
using RatLib;

namespace RatScanner.View;

/// <summary>
/// Interaction logic for BlazorOverlay.xaml
/// </summary>
public partial class BlazorOverlay : Window
{
	[DllImport("user32.dll", SetLastError = true)]
	static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	private const int GwlExStyle = -20;
	private const int WsExToolWindow = 0x00000080;

	// Repeat timer to check for the EFT window and resize if necessary
	private static System.Timers.Timer _windowCheckTimer;

	public BlazorOverlay(ServiceProvider serviceProvider)
	{
		Resources.Add("services", serviceProvider);

		InitializeComponent();
	}

	private void BlazorOverlay_Loaded(object sender, RoutedEventArgs e)
	{
		blazorOverlayWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;

		// Set up the window check timer
		InitializeWindowCheckTimer();

		// Finish loading the WebView
		blazorOverlayWebView.WebView.NavigationCompleted += WebView_Loaded;
		blazorOverlayWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;

		// Hide overlay from alt+tab
		var handle = new WindowInteropHelper(this).Handle;
		SetWindowLong(handle, GwlExStyle, GetWindowLong(handle, GwlExStyle) | WsExToolWindow);
	}

	private void InitializeWindowCheckTimer()
	{
		// Set up the timer with interval
		_windowCheckTimer = new System.Timers.Timer(10000);
		_windowCheckTimer.Elapsed += CheckWindowEvent;
		_windowCheckTimer.AutoReset = false;
		_windowCheckTimer.Enabled = true;

		// Trigger the timer immediately at initialization
		CheckWindowEvent(null, null);
	}

	private void CheckWindowEvent(Object source, ElapsedEventArgs e)
	{
		// Used to find the area of the EFT game client to set up the overlay location and size
		try
		{
			var gameScreenScale = RatScannerMain.Instance?.GameScreenScale;

			if (gameScreenScale == null)
			{
				Logger.LogWarning("Unable to determine the games screen scale!");
				_windowCheckTimer.Start();
				return;
			}

			var gameRect = GameWindowLocator.GetWindowLocation();
			Dispatcher.Invoke(() =>
			{
				this.Left = gameRect.Left;
				this.Top = gameRect.Top;
				this.Width = gameRect.Width / gameScreenScale.Scale;
				this.Height = gameRect.Height / gameScreenScale.Scale;
			});
		}
		catch
		{
			Logger.LogWarning("Did not find the EFT window for the overlay");
		}
		_windowCheckTimer.Start();
	}

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
