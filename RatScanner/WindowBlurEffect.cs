using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RatScanner;

static class WindowBlurEffect
{
	[DllImport("user32.dll")]
	private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

	//private const uint _blurOpacity = 1;
	//private const uint _blurBackgroundColor = 0x0FF000;

	internal enum AccentState
	{
		ACCENT_DISABLED = 0,
		ACCENT_ENABLE_GRADIENT = 1,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_ENABLE_ACRYLICBLURBEHIND = 4, // RS4 1803.17063
		ACCENT_ENABLE_HOSTBACKDROP = 5, // RS5 1809
		ACCENT_INVALID_STATE = 6
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

	private static bool IsTransparencyAvailable()
	{
		// Always available if not on Windows 11
		var version = Environment.OSVersion.Version;
		if (!(version.Major == 10 && version.Build >= 20000)) return true;

		var path = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
		using var key = Registry.CurrentUser.OpenSubKey(path);
		var registryValueObject = key?.GetValue("EnableTransparency");
		if (registryValueObject == null) return false;
		int registryValue = (int)registryValueObject;
		return registryValue == 1;
	}

	internal static void SetBlur(Window window, AccentState accentState)
	{
		if (!IsTransparencyAvailable())
		{
			Logger.LogWarning("Transparency effects not available");
			return;
		}

		var windowHelper = new WindowInteropHelper(window);
		var accent = new AccentPolicy();

		// to enable blur the image behind the window
		accent.AccentState = accentState;
		accent.AccentFlags = 0;
		accent.GradientColor = 0x00_00_00_00;	// A_B_G_R
		accent.AnimationId = 0;

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
