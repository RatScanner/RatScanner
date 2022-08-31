using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;
using MudBlazor.Extensions;

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
		SetWindowStyle();
		blazorOverlayWebView.WebView.NavigationCompleted += WebView_Loaded;
		blazorOverlayWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
	}

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
		NativeMethods.SetWindowPos(handle, 0, left, top, right - left, bottom - top, 0);
	}

	private void SetWindowStyle()
	{
		const int GWL_EXSTYLE = -20;
		const uint WS_EX_TOOLWINDOW = 0x00000080;

		var handle = new WindowInteropHelper(this).Handle;
		NativeMethods.SetWindowLongPtr(handle, GWL_EXSTYLE, NativeMethods.GetWindowLongPtr(handle, GWL_EXSTYLE) | (nint)WS_EX_TOOLWINDOW);
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

	private class NativeMethods
	{
		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
		public static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
		public static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	}
}
