using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using RatScanner.Properties;

namespace RatScanner
{
	internal static class RatConfig
	{
		internal enum Resolution
		{
			WXGA,   // 1366x768
			FHD,    // 1920x1080
			QHD,    // 2560x1440
			UHD,    // 3840x2160
		}

		// Version
		internal static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

		// Paths
		internal static class Paths
		{
			internal static string Base = AppDomain.CurrentDomain.BaseDirectory;
			internal static string Data = Path.Combine(Base, "Data");
			internal static string StaticIcon = Path.Combine(Data, "name");

			private const string EftTempDir = "Battlestate Games\\EscapeFromTarkov\\";
			private static readonly string EftTemp = Path.Combine(Path.GetTempPath(), EftTempDir);
			internal static string DynamicIcon = Path.Combine(EftTemp, "Icon Cache");
			internal static string StaticCorrelation = Path.Combine(Data, "correlation.json");
			internal static string DynamicCorrelation = Path.Combine(DynamicIcon, "index.json");
			internal static string UnknownIcon = Path.Combine(Data, "unknown.png");
			internal static string ConfigFile = Path.Combine(Base, "config.cfg");
			internal static string Debug = Path.Combine(Base, "Debug");
		}

		// Name Scan options
		internal static class NameScan
		{
			internal static bool Enable = true;
			internal static Bitmap Marker = Resources.markerFHD;
			internal static Bitmap MarkerShort = Resources.markerShortFHD;
			internal static float ConfWarnThreshold = 0.90f;
			internal static int MarkerScanSize = 50;
			internal static float MarkerThreshold = 0.9f;
			internal static int TextHorizontalOffset = 21;
			internal static int TextWidth = 500;
			internal static int TextHeight = 17;
		}

		// Icon Scan options
		internal static class IconScan
		{
			internal static bool Enable = true;
			internal static float ConfWarnThreshold = 0.95f;
			internal static bool ScanRotatedIcons = true;
			internal static int ScanPadding => (int)(GetScreenScaleFactor() * 10);
			internal static int ScanWidth => (int)(GetScreenScaleFactor() * 640);
			internal static int ScanHeight => (int)(GetScreenScaleFactor() * 896);
			internal static int ItemSlotSize = 63;
			internal static int ModifierKeyCode = 160; // SHIFT = 160, CTRL = 162, ALT = 164
			internal static bool UseCachedIcons = true;
		}

		// ToolTip options
		internal static class ToolTip
		{
			internal static string DigitGroupingSymbol = ".";
			internal static int Duration = 1500;
			internal static int WidthOffset = 0;
			internal static int HeightOffset = 5;
		}

		// Minimal UI
		internal static class MinimalUi
		{
			internal static bool ShowName = true;
			internal static bool ShowPrice = true;
			internal static bool ShowAvgDayPrice = true;
			internal static bool ShowAvgWeekPrice = true;
			internal static bool ShowPricePerSlot = true;
			internal static bool ShowTraderPrice = true;
			internal static bool ShowUpdated = true;
			internal static int Opacity = 50;
		}

		// Other
		internal static bool LogDebug = false;
		internal static bool MinimizeToTray = false;
		internal static bool AlwaysOnTop = true;

		private static Resolution screenResolution = Resolution.FHD;
		internal static Resolution ScreenResolution
		{
			get => screenResolution;
			set
			{
				screenResolution = value;
				switch (screenResolution)
				{
					case Resolution.WXGA: LoadWXGA(); break;
					case Resolution.FHD: LoadFHD(); break;
					case Resolution.QHD: LoadQHD(); break;
					case Resolution.UHD: LoadUHD(); break;
					default: throw new ArgumentOutOfRangeException(nameof(value), "Unknown screen resolution");
				}
			}
		}

		private static void LoadWXGA()
		{
			throw new NotImplementedException("No WXGA preset defined");
		}

		private static void LoadFHD()
		{
			NameScan.Marker = Resources.markerFHD;
			NameScan.MarkerShort = Resources.markerShortFHD;
			NameScan.MarkerScanSize = 50;
			NameScan.MarkerThreshold = 0.9f;
			NameScan.TextHorizontalOffset = 21;
			NameScan.TextWidth = 500;
			NameScan.TextHeight = 17;

			IconScan.ItemSlotSize = 63;

			ToolTip.HeightOffset = 5;
		}

		private static void LoadQHD()
		{
			NameScan.Marker = Resources.markerQHD;
			NameScan.MarkerShort = Resources.markerShortQHD;
			NameScan.MarkerScanSize = 75;
			NameScan.MarkerThreshold = 0.9f;
			NameScan.TextHorizontalOffset = 28;
			NameScan.TextWidth = 750;
			NameScan.TextHeight = 23;

			IconScan.ItemSlotSize = 84;

			ToolTip.HeightOffset = 8;
		}

		private static void LoadUHD()
		{
			NameScan.Marker = Resources.markerUHD;
			NameScan.MarkerShort = Resources.markerShortUHD;
			NameScan.MarkerScanSize = 100;
			NameScan.MarkerThreshold = 0.9f;
			NameScan.TextHorizontalOffset = 40;
			NameScan.TextWidth = 1000;
			NameScan.TextHeight = 34;

			IconScan.ItemSlotSize = 126;

			ToolTip.HeightOffset = 10;
		}

		internal static float GetScreenScaleFactor()
		{
			return ScreenResolution switch
			{
				Resolution.WXGA => 768f / 1080f,
				Resolution.FHD => 1f,
				Resolution.QHD => 1440f / 1080f,
				Resolution.UHD => 2f,
				_ => throw new InvalidOperationException("Unknown ScreenResolution"),
			};
		}

		internal static float GetInverseScreenScaleFactor()
		{
			return 1 / GetScreenScaleFactor();
		}

		internal static void LoadConfig()
		{
			if (!File.Exists(Paths.ConfigFile)) SaveConfig();

			var config = new SimpleConfig(Paths.ConfigFile);
			NameScan.Enable = config.ReadBool(nameof(NameScan.Enable), true);

			IconScan.Enable = config.ReadBool(nameof(IconScan.Enable), true);
			IconScan.ScanRotatedIcons = config.ReadBool(nameof(IconScan.ScanRotatedIcons), true);
			IconScan.ModifierKeyCode = config.ReadInt(nameof(IconScan.ModifierKeyCode), 160);
			IconScan.UseCachedIcons = config.ReadBool(nameof(IconScan.UseCachedIcons), true);

			ToolTip.Duration = config.ReadInt(nameof(ToolTip.Duration), 1500);
			ToolTip.DigitGroupingSymbol = config.ReadString(nameof(ToolTip.DigitGroupingSymbol), NumberFormatInfo.CurrentInfo.NumberGroupSeparator);

			MinimalUi.ShowName = config.ReadBool(nameof(MinimalUi.ShowName), true);
			MinimalUi.ShowPrice = config.ReadBool(nameof(MinimalUi.ShowPrice), true);
			MinimalUi.ShowAvgDayPrice = config.ReadBool(nameof(MinimalUi.ShowAvgDayPrice), true);
			MinimalUi.ShowAvgWeekPrice = config.ReadBool(nameof(MinimalUi.ShowAvgWeekPrice), true);
			MinimalUi.ShowPricePerSlot = config.ReadBool(nameof(MinimalUi.ShowPricePerSlot), true);
			MinimalUi.ShowTraderPrice = config.ReadBool(nameof(MinimalUi.ShowTraderPrice), true);
			MinimalUi.ShowUpdated = config.ReadBool(nameof(MinimalUi.ShowUpdated), true);
			MinimalUi.Opacity = config.ReadInt(nameof(MinimalUi.Opacity), 50);

			ScreenResolution = (Resolution)config.ReadInt(nameof(ScreenResolution), 1);
			MinimizeToTray = config.ReadBool(nameof(MinimizeToTray), false);
			AlwaysOnTop = config.ReadBool(nameof(AlwaysOnTop), false);
			LogDebug = config.ReadBool(nameof(LogDebug), false);
		}

		internal static void SaveConfig()
		{
			var config = new SimpleConfig(Paths.ConfigFile);
			config.WriteBool(nameof(NameScan.Enable), NameScan.Enable);

			config.WriteBool(nameof(IconScan.Enable), IconScan.Enable);
			config.WriteBool(nameof(IconScan.ScanRotatedIcons), IconScan.ScanRotatedIcons);
			config.WriteInt(nameof(IconScan.ModifierKeyCode), IconScan.ModifierKeyCode);
			config.WriteBool(nameof(IconScan.UseCachedIcons), IconScan.UseCachedIcons);

			config.WriteInt(nameof(ToolTip.Duration), ToolTip.Duration);
			config.WriteString(nameof(ToolTip.DigitGroupingSymbol), ToolTip.DigitGroupingSymbol);

			config.WriteBool(nameof(MinimalUi.ShowName), MinimalUi.ShowName);
			config.WriteBool(nameof(MinimalUi.ShowPrice), MinimalUi.ShowPrice);
			config.WriteBool(nameof(MinimalUi.ShowAvgDayPrice), MinimalUi.ShowAvgDayPrice);
			config.WriteBool(nameof(MinimalUi.ShowAvgWeekPrice), MinimalUi.ShowAvgWeekPrice);
			config.WriteBool(nameof(MinimalUi.ShowPricePerSlot), MinimalUi.ShowPricePerSlot);
			config.WriteBool(nameof(MinimalUi.ShowTraderPrice), MinimalUi.ShowTraderPrice);
			config.WriteBool(nameof(MinimalUi.ShowUpdated), MinimalUi.ShowUpdated);
			config.WriteInt(nameof(MinimalUi.Opacity), MinimalUi.Opacity);

			config.WriteInt(nameof(ScreenResolution), (int)ScreenResolution);
			config.WriteBool(nameof(MinimizeToTray), MinimizeToTray);
			config.WriteBool(nameof(AlwaysOnTop), AlwaysOnTop);
			config.WriteBool(nameof(LogDebug), LogDebug);
		}
	}
}
