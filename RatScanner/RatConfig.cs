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

		// String resources
		internal static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
		internal static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
		internal static string DataPath = Path.Combine(BasePath, "Data");
		internal static string StaticIconPath = Path.Combine(DataPath, "name");
		private const string EftTempDir = "Battlestate Games\\EscapeFromTarkov\\";
		private static readonly string EftTempPath = Path.Combine(Path.GetTempPath(), EftTempDir);
		internal static string DynamicIconPath = Path.Combine(EftTempPath, "Icon Cache");
		internal static string StaticCorrelationPath = Path.Combine(DataPath, "correlation.json");
		internal static string DynamicCorrelationPath = Path.Combine(DynamicIconPath, "index.json");
		internal static string UnknownIconPath = Path.Combine(DataPath, "unknown.png");
		internal static string ConfigFilePath = Path.Combine(BasePath, "config.cfg");
		internal static string DebugPath = Path.Combine(BasePath, "Debug");

		// Name Scan options
		internal static Bitmap Marker = Resources.markerFHD;
		internal static Bitmap MarkerShort = Resources.markerShortFHD;
		internal static bool EnableNameScan = true;
		internal static float NameConfWarnThreshold = 0.90f;
		internal static int MarkerScanSize = 50;
		internal static float MarkerThreshold = 0.9f;
		internal static int TextHorizontalOffset = 21;
		internal static int TextWidth = 500;
		internal static int TextHeight = 17;

		// Icon Scan options
		internal static bool EnableIconScan = true;
		internal static float IconConfWarnThreshold = 0.95f;
		internal static bool ScanRotatedIcons = true;
		internal static int IconPadding = 10;
		internal static int IconScanWidth = 640;
		internal static int IconScanHeight = 896;
		internal static int ItemSlotSize = 63;
		internal static int ModifierKeyCode = 160; // SHIFT = 160, CTRL = 162, ALT = 164
		internal static bool UseCachedIcons = true;

		// ToolTip options
		internal static string ToolTipDigitGroupingSymbol = ".";
		internal static int ToolTipDuration = 1500;
		internal static int BToolTipWidthOffset = 0;
		internal static int BToolTipHeightOffset = 5;

		// Other
		internal static bool LogDebug = false;
		internal static bool MinimizeToTray = false;
		internal static bool AlwaysOnTop = false;

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
			Marker = Resources.markerFHD;
			MarkerShort = Resources.markerShortFHD;
			MarkerScanSize = 50;
			MarkerThreshold = 0.9f;
			TextHorizontalOffset = 21;
			TextWidth = 500;
			TextHeight = 17;

			ItemSlotSize = 63;

			BToolTipHeightOffset = 5;
		}

		private static void LoadQHD()
		{
			Marker = Resources.markerQHD;
			MarkerShort = Resources.markerShortQHD;
			MarkerScanSize = 75;
			MarkerThreshold = 0.9f;
			TextHorizontalOffset = 28;
			TextWidth = 750;
			TextHeight = 23;

			ItemSlotSize = 84;

			BToolTipHeightOffset = 8;
		}

		private static void LoadUHD()
		{
			Marker = Resources.markerUHD;
			MarkerShort = Resources.markerShortUHD;
			MarkerScanSize = 100;
			MarkerThreshold = 0.9f;
			TextHorizontalOffset = 40;
			TextWidth = 1000;
			TextHeight = 34;

			ItemSlotSize = 126;

			BToolTipHeightOffset = 10;
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
			if (!File.Exists(ConfigFilePath)) SaveConfig();

			var config = new SimpleConfig(ConfigFilePath);
			EnableNameScan = config.ReadBool(nameof(EnableNameScan), true);
			EnableIconScan = config.ReadBool(nameof(EnableIconScan), true);
			ScanRotatedIcons = config.ReadBool(nameof(ScanRotatedIcons), true);
			ToolTipDuration = config.ReadInt(nameof(ToolTipDuration), 1500);
			ModifierKeyCode = config.ReadInt(nameof(ModifierKeyCode), 160);
			LogDebug = config.ReadBool(nameof(LogDebug), false);
			MinimizeToTray = config.ReadBool(nameof(MinimizeToTray), false);
			AlwaysOnTop = config.ReadBool(nameof(AlwaysOnTop), false);
			ScreenResolution = (Resolution)config.ReadInt(nameof(ScreenResolution), 1);
			ToolTipDigitGroupingSymbol = config.ReadString(nameof(ToolTipDigitGroupingSymbol), NumberFormatInfo.CurrentInfo.NumberGroupSeparator);
			UseCachedIcons = config.ReadBool(nameof(UseCachedIcons), true);
		}

		internal static void SaveConfig()
		{
			var config = new SimpleConfig(ConfigFilePath);
			config.WriteBool(nameof(EnableNameScan), EnableNameScan);
			config.WriteBool(nameof(EnableIconScan), EnableIconScan);
			config.WriteBool(nameof(ScanRotatedIcons), ScanRotatedIcons);
			config.WriteInt(nameof(ToolTipDuration), ToolTipDuration);
			config.WriteInt(nameof(ModifierKeyCode), ModifierKeyCode);
			config.WriteBool(nameof(LogDebug), LogDebug);
			config.WriteBool(nameof(MinimizeToTray), MinimizeToTray);
			config.WriteBool(nameof(AlwaysOnTop), AlwaysOnTop);
			config.WriteInt(nameof(ScreenResolution), (int)ScreenResolution);
			config.WriteString(nameof(ToolTipDigitGroupingSymbol), ToolTipDigitGroupingSymbol);
			config.WriteBool(nameof(UseCachedIcons), UseCachedIcons);
		}
	}
}