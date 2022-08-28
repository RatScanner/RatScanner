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
	public static void GetDpi(this System.Windows.Forms.Screen screen, DpiType dpiType, out uint dpiX, out uint dpiY)
	{
		var pnt = new System.Drawing.Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
		var mon = MonitorFromPoint(pnt, 2/*MONITOR_DEFAULTTONEAREST*/);
		GetDpiForMonitor(mon, dpiType, out dpiX, out dpiY);
	}

	public static double GetScalingForPoint(System.Drawing.Point aPoint)
	{
		var mon = MonitorFromPoint(aPoint, 2/*MONITOR_DEFAULTTONEAREST*/);
		uint dpiX, dpiY;
		GetDpiForMonitor(mon, DpiType.Effective, out dpiX, out dpiY);
		return (double)dpiX / 96.0;
	}


	[DllImport(User32)]
	private static extern IntPtr MonitorFromPoint([In] System.Drawing.Point pt, [In] uint dwFlags);


	[DllImport(Shcore)]
	private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

	[DllImport(User32, CharSet = CharSet.Auto)]
	[ResourceExposure(ResourceScope.None)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

	[DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
	[ResourceExposure(ResourceScope.None)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);

	public enum DpiType
	{
		Effective = 0,
		Angular = 1,
		Raw = 2,
	}

	public static WindowPlacement GetPlacement(IntPtr hWnd)
	{
		WindowPlacement placement = new WindowPlacement();
		placement.length = Marshal.SizeOf(placement);
		GetWindowPlacement(hWnd, ref placement);
		return placement;
	}

	public static bool SetPlacement(IntPtr hWnd, WindowPlacement aPlacement)
	{
		bool erg = SetWindowPlacement(hWnd, ref aPlacement);
		return erg;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PointStruct
	{
		public int x;
		public int y;
		public PointStruct(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Rect
	{
		public int left;
		public int top;
		public int right;
		public int bottom;

		public Rect(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		public Rect(System.Windows.Rect r)
		{
			this.left = (int)r.Left;
			this.top = (int)r.Top;
			this.right = (int)r.Right;
			this.bottom = (int)r.Bottom;
		}

		public static Rect FromXywh(int x, int y, int width, int height)
		{
			return new Rect(x, y, x + width, y + height);
		}

		public Size Size
		{
			get { return new Size(this.right - this.left, this.bottom - this.top); }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WindowPlacement
	{
		public int length;
		public uint flags;
		public uint showCmd;
		public PointStruct ptMinPosition;
		public PointStruct ptMaxPosition;
		public Rect rcNormalPosition;

		public override string ToString()
		{
			byte[] structBytes = RawSerialize(this);
			return System.Convert.ToBase64String(structBytes);
		}

		public void ReadFromBase64String(string aB64)
		{
			byte[] b64 = System.Convert.FromBase64String(aB64);
			var newWp = ReadStruct<WindowPlacement>(b64, 0);
			length = newWp.length;
			flags = newWp.flags;
			showCmd = newWp.showCmd;
			ptMinPosition.x = newWp.ptMinPosition.x;
			ptMinPosition.y = newWp.ptMinPosition.y;
			ptMaxPosition.x = newWp.ptMaxPosition.x;
			ptMaxPosition.y = newWp.ptMaxPosition.y;
			rcNormalPosition.left = newWp.rcNormalPosition.left;
			rcNormalPosition.top = newWp.rcNormalPosition.top;
			rcNormalPosition.right = newWp.rcNormalPosition.right;
			rcNormalPosition.bottom = newWp.rcNormalPosition.bottom;
		}

		static public T ReadStruct<T>(byte[] aSrcBuffer, int aOffset)
		{
			byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
			Buffer.BlockCopy(aSrcBuffer, aOffset, buffer, 0, Marshal.SizeOf(typeof(T)));
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return temp;
		}

		static public T ReadStruct<T>(Stream fs)
		{
			byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
			fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return temp;
		}

		public static byte[] RawSerialize(object anything)
		{
			int rawsize = Marshal.SizeOf(anything);
			byte[] rawdata = new byte[rawsize];
			GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
			Marshal.StructureToPtr(anything, handle.AddrOfPinnedObject(), false);
			handle.Free();
			return rawdata;
		}
	}

	public static List<ScreenScale> GetScreenScales()
	{
		List<ScreenScale> scales = new List<ScreenScale>();
		// Look through all screens and calculate the scaling on each screen and return that as a screenscale with bounds
		foreach (Screen screen in Screen.AllScreens)
		{

			double scale = GetScalingForPoint(new System.Drawing.Point(screen.Bounds.X, screen.Bounds.Y));

			scales.Add(new ScreenScale(scale, screen.Bounds));
		}
		return scales;
	}

	public static ScreenScale? GameWindowScreenScale()
	{
		try
		{
			System.Drawing.Rectangle gameRect = GameWindowLocator.GetWindowLocation();
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
