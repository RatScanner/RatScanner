using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Forms;
using RatLib;

namespace RatScanner;

public static class ScreenInfo
{
	public const string User32 = "user32.dll";
	public const string Shcore = "Shcore.dll";


	[DllImport(User32)]
	private static extern IntPtr MonitorFromPoint([In] System.Drawing.Point pt, [In] uint dwFlags);


	[DllImport(Shcore)]
	private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

	public enum DpiType
	{
		Effective = 0,
		Angular = 1,
		Raw = 2,
	}

	public static double GetScalingForPoint(System.Drawing.Point aPoint)
	{
		var mon = MonitorFromPoint(aPoint, 2/*MONITOR_DEFAULTTONEAREST*/);
		uint dpiX, dpiY;
		GetDpiForMonitor(mon, DpiType.Effective, out dpiX, out dpiY);
		return (double)dpiX / 96.0;
	}

	public static List<ScreenScale> GetScreenScales()
	{
		var scales = new List<ScreenScale>();
		// Look through all screens and calculate the scaling on each screen and return that as a screenscale with bounds
		foreach (var screen in Screen.AllScreens)
		{

			var scale = GetScalingForPoint(new System.Drawing.Point(screen.Bounds.X, screen.Bounds.Y));

			scales.Add(new ScreenScale(scale, screen.Bounds));
		}
		return scales;
	}

	public static ScreenScale? GameWindowScreenScale()
	{
		try
		{
			var gameRect = GameWindowLocator.GetWindowLocation();
			// Find the ScreenScale where the bounds of the EFT game window are within the bounds of the ScreenScale
			// The gameRect is screwed up. The Width is actually the right hand bound, and the Height is the lower bound
			// So we have tod so some special checks instead of a .Contains(gameRect) call.
			return GetScreenScales().Where(s => s.Bounds.Contains(gameRect)).First();
		}
		catch
		{
			// Did not find the screen, or we dont have tarkov
			return null;
		}
	}
}
