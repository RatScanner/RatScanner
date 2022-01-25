using System;
using System.Runtime.InteropServices;

namespace RatScanner.View;

public static class WindowsServices
{
	private const int WsExTransparent = 0x00000020;
	private const int GwlExStyle = -20;

	[DllImport("user32.dll")]
	private static extern int GetWindowLong(IntPtr hwnd, int index);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

	public static void SetWindowExTransparent(IntPtr hwnd)
	{
		var extendedStyle = GetWindowLong(hwnd, GwlExStyle);
		SetWindowLong(hwnd, GwlExStyle, extendedStyle | WsExTransparent);
	}
}
