using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using RatScanner.Properties;

namespace RatScanner
{
	internal static class RatConfig
	{
		internal enum Resolution
		{
			R1366x768  = 0,
			R1440x900  = 1,
			R1440x1080 = 2,
			R1600x900  = 3,
			R1920x1080 = 4,
			R2560x1440 = 5,
			R3840x2160 = 6,
			R2560x1080 = 7,
			R3840x1080 = 8,
			R3440x1440 = 9,
			R5120x1440 = 10
		}

		internal static Dictionary<Resolution, Vector2> ResolutionDict = new Dictionary<Resolution, Vector2>()
		{
			{Resolution.R1366x768, new Vector2(1366,  768)},
			{Resolution.R1440x900, new Vector2(1440,  900)},
			{Resolution.R1440x1080, new Vector2(1440, 1080)},
			{Resolution.R1600x900, new Vector2(1600,  900)},
			{Resolution.R1920x1080, new Vector2(1920, 1080)},
			{Resolution.R2560x1440, new Vector2(2560, 1440)},
			{Resolution.R3840x2160, new Vector2(3840, 2160)},
			{Resolution.R2560x1080, new Vector2(2560, 1080)},
			{Resolution.R3840x1080, new Vector2(3840, 1080)},
			{Resolution.R3440x1440, new Vector2(3440, 1440)},
			{Resolution.R5120x1440, new Vector2(5210, 1440)},
		};

		// Version
		public static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

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
			internal static string QuestItemPath = Path.Combine(Data, "questItems.json");
			internal static string DynamicCorrelation = Path.Combine(DynamicIcon, "index.json");
			internal static string UnknownIcon = Path.Combine(Data, "unknown.png");
			internal static string ConfigFile = Path.Combine(Base, "config.cfg");
			internal static string Debug = Path.Combine(Base, "Debug");
			internal static string Updater = Path.Combine(Base, "RatUpdater.exe");
			internal static string LogFile = Path.Combine(Base, "Log.txt");
		}

		// Name Scan options
		internal static class NameScan
		{
			internal static bool Enable = true;
			internal static ApiManager.Language Language = ApiManager.Language.English;
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

		private static Resolution screenResolution = Resolution.R1920x1080;
		internal static Resolution ScreenResolution
		{
			get => screenResolution;
			set
			{
				screenResolution = value;
				switch (screenResolution)
				{
					case Resolution.R1366x768:  LoadWXGA(); break;
					case Resolution.R1440x900:  LoadWXGAPlus(); break;
					case Resolution.R1440x1080: LoadR1440x1080(); break;
					case Resolution.R1600x900:  LoadHDPlus(); break;
					case Resolution.R1920x1080: LoadFHD(); break;
					case Resolution.R2560x1080: LoadFHD(); break;
					case Resolution.R3840x1080: LoadFHD(); break;
					case Resolution.R2560x1440: LoadQHD(); break;
					case Resolution.R3440x1440: LoadQHD(); break;
					case Resolution.R5120x1440: LoadQHD(); break;
					case Resolution.R3840x2160: LoadUHD(); break;
					default: throw new ArgumentOutOfRangeException(nameof(value), "Unknown screen resolution");
				}

				UpdateResolutionSettings();
			}
		}

		private static void UpdateResolutionSettings()
		{
			var marker = NameScan.Marker;
			NameScan.MarkerScanSize = Math.Max(marker.Width, marker.Height) * 3;
			NameScan.TextHorizontalOffset = (int)(marker.Width * 1.2f);
			NameScan.TextHeight = marker.Height;
			NameScan.TextWidth = (int)(GetScreenScaleFactor() * 600f);
		}

		#region Resolution presets
		private static void LoadWXGA()
		{
			NameScan.Marker = Resources.markerWXGA;
			// TODO NameScan.MarkerShort = ...;
			IconScan.ItemSlotSize = 45;
			ToolTip.HeightOffset = -1;
		}

		private static void LoadWXGAPlus()
		{
			NameScan.Marker = Resources.markerWXGAPlus;
			// TODO NameScan.MarkerShort = ...;
			IconScan.ItemSlotSize = 49;
			ToolTip.HeightOffset = 0;
		}

		private static void LoadR1440x1080()
		{
			NameScan.Marker = Resources.markerR1440x1080;
			// TODO NameScan.MarkerShort = ...;
			IconScan.ItemSlotSize = 47;
			ToolTip.HeightOffset = 0;
		}

		private static void LoadHDPlus()
		{
			NameScan.Marker = Resources.markerHDPlus;
			// TODO NameScan.MarkerShort = ...;
			IconScan.ItemSlotSize = 51;
			ToolTip.HeightOffset = 2;
		}

		private static void LoadFHD()
		{
			NameScan.Marker = Resources.markerFHD;
			NameScan.MarkerShort = Resources.markerShortFHD;
			IconScan.ItemSlotSize = 63;
			ToolTip.HeightOffset = 5;
		}

		private static void LoadQHD()
		{
			NameScan.Marker = Resources.markerQHD;
			NameScan.MarkerShort = Resources.markerShortQHD;
			IconScan.ItemSlotSize = 84;
			ToolTip.HeightOffset = 11;
		}

		private static void LoadUHD()
		{
			NameScan.Marker = Resources.markerUHD;
			NameScan.MarkerShort = Resources.markerShortUHD;
			IconScan.ItemSlotSize = 126;
			ToolTip.HeightOffset = 10;
		}
		#endregion

		internal static float GetScreenScaleFactor()
		{
			var screenDimension = ResolutionDict[ScreenResolution];

			var screenScaleFactor1 = screenDimension.X / 1920f;
			var screenScaleFactor2 = screenDimension.Y / 1080f;

			return Math.Min(screenScaleFactor1, screenScaleFactor2);
		}

		internal static float GetInverseScreenScaleFactor()
		{
			return 1 / GetScreenScaleFactor();
		}

		internal static void LoadConfig()
		{
			if (!File.Exists(Paths.ConfigFile))
			{
				TrySetScreenResolution();
				SaveConfig();
			}

			var config = new SimpleConfig(Paths.ConfigFile);
			NameScan.Enable = config.ReadBool(nameof(NameScan.Enable), true);
			NameScan.Language = (ApiManager.Language)config.ReadInt(nameof(NameScan.Language), (int)ApiManager.Language.English);

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

			ScreenResolution = (Resolution)config.ReadInt(nameof(ScreenResolution), (int)Resolution.R1920x1080);
			MinimizeToTray = config.ReadBool(nameof(MinimizeToTray), false);
			AlwaysOnTop = config.ReadBool(nameof(AlwaysOnTop), false);
			LogDebug = config.ReadBool(nameof(LogDebug), false);
		}

		internal static void SaveConfig()
		{
			var config = new SimpleConfig(Paths.ConfigFile);
			config.WriteBool(nameof(NameScan.Enable), NameScan.Enable);
			config.WriteInt(nameof(NameScan.Language), (int)NameScan.Language);

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

		/// <summary>
		/// Converts PrimaryScreen resolution to Resolution enum, sets screenResolution if a match is found
		/// </summary>
		internal static void TrySetScreenResolution()
		{
			var boundsRectangle = Screen.PrimaryScreen.Bounds;
			var resolutionString = $"R{boundsRectangle.Width}x{boundsRectangle.Height}";
			Enum.TryParse(typeof(Resolution), resolutionString, out var matchingResolution);
			if (matchingResolution != null)
			{
				screenResolution = (Resolution) matchingResolution;
			}
		}
	}
}
