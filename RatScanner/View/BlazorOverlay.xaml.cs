using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;

namespace RatScanner.View;

/// <summary>
/// Interaction logic for BlazorOverlay.xaml
/// </summary>
public partial class BlazorOverlay : Window
{
	public BlazorOverlay(ServiceProvider serviceProvider)
	{
		Resources.Add("services", serviceProvider);

		InitializeComponent();
	}

	private void BlazorOverlay_Loaded(object sender, RoutedEventArgs e)
	{
		blazorOverlayWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
		SetSize();
		blazorOverlayWebView.WebView.NavigationCompleted += WebView_Loaded;
		blazorOverlayWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
	}

	[DllImport("user32.dll", SetLastError = true)]
	static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, UInt32 uFlags);

	private void SetSize()
	{
		var bounds = Screen.AllScreens.Select(screen => screen.Bounds);
		var left = 0;
		var top = 0;
		var right = 0;
		var bottom = 0;
		foreach (var bound in bounds)
		{
			if (bound.Left < left) left = bound.Left;
			if (bound.Top < top) top = bound.Top;
			if (bound.Right > right) right = bound.Right;
			if (bound.Bottom > bottom) bottom = bound.Bottom;
		}
		
		var handle = new WindowInteropHelper(this).Handle;
		SetWindowPos(handle, (IntPtr)0, left, top, right - left, bottom - top, 0);
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
