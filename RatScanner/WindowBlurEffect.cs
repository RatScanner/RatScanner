using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RatScanner;

static class WindowBlurEffect
{
	[DllImport("user32.dll")]
	private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

	private const uint _blurOpacity = 1;
	private const uint _blurBackgroundColor = 0x000000;

	internal enum AccentState
	{
		ACCENT_DISABLED = 0,
		ACCENT_ENABLE_GRADIENT = 1,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
		ACCENT_INVALID_STATE = 5
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct AccentPolicy
	{
		public AccentState AccentState;
		public uint AccentFlags;
		public uint GradientColor;
		public uint AnimationId;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct WindowCompositionAttributeData
	{
		public WindowCompositionAttribute Attribute;
		public IntPtr Data;
		public int SizeOfData;
	}

	private enum WindowCompositionAttribute
	{
		WCA_ACCENT_POLICY = 19
	}

	internal static void EnableBlur(Window window, AccentState accentState)
	{
		var windowHelper = new WindowInteropHelper(window);
		var accent = new AccentPolicy();


		// to enable blur the image behind the window
		accent.AccentState = accentState;
		accent.AccentFlags = 0b10;
		accent.GradientColor = (_blurOpacity << 24) | (_blurBackgroundColor & 0xFFFFFF); /*(White mask 0xFFFFFF)*/


		var accentStructSize = Marshal.SizeOf(accent);

		var accentPtr = Marshal.AllocHGlobal(accentStructSize);
		Marshal.StructureToPtr(accent, accentPtr, false);

		var data = new WindowCompositionAttributeData();
		data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
		data.SizeOfData = accentStructSize;
		data.Data = accentPtr;

		SetWindowCompositionAttribute(windowHelper.Handle, ref data);

		Marshal.FreeHGlobal(accentPtr);
	}
}
