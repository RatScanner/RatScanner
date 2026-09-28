using System;
using System.Runtime.InteropServices;

namespace RatScanner.View;

public static partial class WindowsServices {
	private const int WsExTransparent = 0x00000020;
	private const int GwlExStyle = -20;

	[LibraryImport("user32.dll", EntryPoint = "GetWindowLongW")]
	private static partial int GetWindowLong(IntPtr hwnd, int index);

	[LibraryImport("user32.dll", EntryPoint = "SetWindowLongW")]
	private static partial int SetWindowLong(IntPtr hwnd, int index, int newStyle);

	public static void SetWindowExTransparent(IntPtr hwnd) {
		var extendedStyle = GetWindowLong(hwnd, GwlExStyle);
		_ = SetWindowLong(hwnd, GwlExStyle, extendedStyle | WsExTransparent);
	}
}
