using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;
using RatScanner.Controls;
using RatScanner.Properties;

namespace RatScanner
{
	internal static class RatConfig
	{
		internal enum Resolution
		{
			R1366x768 = 0,
			R1440x900 = 1,
			R1440x1080 = 2,
			R1600x900 = 3,
			R1920x1080 = 4,
			R2560x1440 = 5,
			R3840x2160 = 6,
			R2560x1080 = 7,
			R3840x1080 = 8,
			R3440x1440 = 9,
			R5120x1440 = 10,
		}

		internal static Dictionary<Resolution, Vector2> ResolutionDict = new Dictionary<Resolution, Vector2>()
		{
			{Resolution.R1366x768, new Vector2(1366, 768)},
			{Resolution.R1440x900, new Vector2(1440, 900)},
			{Resolution.R1440x1080, new Vector2(1440, 1080)},
			{Resolution.R1600x900, new Vector2(1600, 900)},
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
			internal static string DynamicCorrelation = Path.Combine(DynamicIcon, "index.json");
			internal static string ItemData = Path.Combine(Data, "items.json");
			internal static string UnknownIcon = Path.Combine(Data, "unknown.png");
			internal static string ConfigFile = Path.Combine(Base, "config.cfg");
			internal static string Debug = Path.Combine(Base, "Debug");
			internal static string Updater = Path.Combine(Base, "RatUpdater.exe");
			internal static string LogFile = Path.Combine(Base, "Log.txt");
			internal static string Wishlist = Path.Combine(Data, "Wishlist.json");
		}

		// Name Scan options
		internal static class NameScan
		{
			internal static bool Enable = true;
			internal static ApiManager.Language Language = ApiManager.Language.English;
			internal static Bitmap Marker = Resources.markerFHD;
			internal static Bitmap MarkerShort = Resources.markerShortFHD;
			internal static float ConfWarnThreshold = 0.85f;
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
			internal static int ScanWidth => (int)(GetScreenScaleFactor() * 896);
			internal static int ScanHeight => (int)(GetScreenScaleFactor() * 896);
			internal static int ItemSlotSize = 63;
			internal static Hotkey Hotkey = new Hotkey(new[] { Key.LeftShift }.ToList(), new[] { MouseButton.Left });
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
			internal static bool ShowAvgDayPrice = true;
			internal static bool ShowPricePerSlot = true;
			internal static bool ShowTraderPrice = true;
			internal static bool ShowTraderMaxPrice = false;
			internal static bool ShowUpdated = false;
			internal static bool ShowQuestHideoutTracker = true;
			internal static bool ShowQuestHideoutTeamTracker = false;
			internal static int Opacity = 10;
		}

		// Progress Tracking options
		internal static class Tracking
		{
			internal static bool ShowNonFIRNeeds = true;

			internal static class TarkovTracker
			{
				internal static bool Enable => Token.Length > 0;

				internal static string Token = "";
				internal static bool ShowTeam = true;
				internal static int RefreshTime = 10 * 60 * 1000;   // 10 minutes
			}
		}

		// Other
#if DEBUG
		internal static bool LogDebug
		{
			get => true;
			set { }
		}
#else
		internal static bool LogDebug = false;
#endif
		internal static bool MinimizeToTray = false;
		internal static bool AlwaysOnTop = true;
		internal static int MarketDBRefreshTime = 60 * 60 * 1000;   // 1 hour
		internal static string ItemDataVersion = "20200101";
		private static int ConfigVersion => 1;

		private static Resolution screenResolution = Resolution.R1920x1080;

		internal static Resolution ScreenResolution
		{
			get => screenResolution;
			set
			{
				screenResolution = value;
				switch (screenResolution)
				{
					case Resolution.R1366x768: LoadWXGA(); break;
					case Resolution.R1440x900: LoadWXGAPlus(); break;
					case Resolution.R1440x1080: LoadR1440x1080(); break;
					case Resolution.R1600x900: LoadHDPlus(); break;
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

		private static bool IsSupportedConfigVersion()
		{
			var config = new SimpleConfig(Paths.ConfigFile, "Other");
			var readConfigVersion = config.ReadInt(nameof(ConfigVersion), -1);
			var isSupportedConfigVersion = ConfigVersion == readConfigVersion;
			if (!isSupportedConfigVersion) Logger.LogWarning("Config version (" + readConfigVersion + ") is not supported!");
			return isSupportedConfigVersion;
		}

		internal static void LoadConfig()
		{
			var configFileExists = File.Exists(Paths.ConfigFile);
			var isSupportedConfigVersion = IsSupportedConfigVersion();
			if (configFileExists && !isSupportedConfigVersion)
			{
				var message = "Old config version detected!\n\n";
				message += "It will be removed and replaced with a new config file.\n";
				message += "Please make sure to reconfigure your settings after.";
				Logger.ShowMessage(message);

				File.Delete(Paths.ConfigFile);
				TrySetScreenResolution();
				SaveConfig();
			}
			else if (!configFileExists)
			{
				TrySetScreenResolution();
				SaveConfig();
			}

			var config = new SimpleConfig(Paths.ConfigFile);

			config.Section = nameof(NameScan);
			NameScan.Enable = config.ReadBool(nameof(NameScan.Enable), true);
			NameScan.Language = (ApiManager.Language)config.ReadInt(nameof(NameScan.Language), (int)ApiManager.Language.English);

			config.Section = nameof(IconScan);
			IconScan.Enable = config.ReadBool(nameof(IconScan.Enable), true);
			IconScan.ScanRotatedIcons = config.ReadBool(nameof(IconScan.ScanRotatedIcons), true);
			var keyboardKeys = config.ReadEnumerableEnum(nameof(IconScan.Hotkey) + "Keyboard", new[] { Key.LeftShift });
			var mouseButtons = config.ReadEnumerableEnum(nameof(IconScan.Hotkey) + "Mouse", new[] { MouseButton.Left });
			IconScan.Hotkey = new Hotkey(keyboardKeys.ToList(), mouseButtons.ToList());
			IconScan.UseCachedIcons = config.ReadBool(nameof(IconScan.UseCachedIcons), true);

			config.Section = nameof(ToolTip);
			ToolTip.Duration = config.ReadInt(nameof(ToolTip.Duration), 1500);
			ToolTip.DigitGroupingSymbol = config.ReadString(nameof(ToolTip.DigitGroupingSymbol), NumberFormatInfo.CurrentInfo.NumberGroupSeparator);

			config.Section = nameof(MinimalUi);
			MinimalUi.ShowName = config.ReadBool(nameof(MinimalUi.ShowName), true);
			MinimalUi.ShowAvgDayPrice = config.ReadBool(nameof(MinimalUi.ShowAvgDayPrice), true);
			MinimalUi.ShowPricePerSlot = config.ReadBool(nameof(MinimalUi.ShowPricePerSlot), true);
			MinimalUi.ShowTraderPrice = config.ReadBool(nameof(MinimalUi.ShowTraderPrice), true);
			MinimalUi.ShowTraderMaxPrice = config.ReadBool(nameof(MinimalUi.ShowTraderMaxPrice), true);
			MinimalUi.ShowUpdated = config.ReadBool(nameof(MinimalUi.ShowUpdated), true);
			MinimalUi.ShowQuestHideoutTracker = config.ReadBool(nameof(MinimalUi.ShowQuestHideoutTracker), true);
			MinimalUi.ShowQuestHideoutTeamTracker = config.ReadBool(nameof(MinimalUi.ShowQuestHideoutTeamTracker), true);
			MinimalUi.Opacity = config.ReadInt(nameof(MinimalUi.Opacity), 50);

			config.Section = nameof(Tracking);
			Tracking.ShowNonFIRNeeds = config.ReadBool(nameof(Tracking.ShowNonFIRNeeds), true);

			config.Section = nameof(Tracking.TarkovTracker);
			Tracking.TarkovTracker.Token = config.ReadString(nameof(Tracking.TarkovTracker.Token), "");
			Tracking.TarkovTracker.ShowTeam = config.ReadBool(nameof(Tracking.TarkovTracker.ShowTeam), true);

			config.Section = "Other";
			ScreenResolution = (Resolution)config.ReadInt(nameof(ScreenResolution), (int)Resolution.R1920x1080);
			MinimizeToTray = config.ReadBool(nameof(MinimizeToTray), false);
			AlwaysOnTop = config.ReadBool(nameof(AlwaysOnTop), false);
			ItemDataVersion = config.ReadString(nameof(ItemDataVersion), "20200101");
			LogDebug = config.ReadBool(nameof(LogDebug), false);
		}

		internal static void SaveConfig()
		{
			var config = new SimpleConfig(Paths.ConfigFile);

			config.Section = nameof(NameScan);
			config.WriteBool(nameof(NameScan.Enable), NameScan.Enable);
			config.WriteInt(nameof(NameScan.Language), (int)NameScan.Language);

			config.Section = nameof(IconScan);
			config.WriteBool(nameof(IconScan.Enable), IconScan.Enable);
			config.WriteBool(nameof(IconScan.ScanRotatedIcons), IconScan.ScanRotatedIcons);
			config.WriteEnumerableEnum(nameof(IconScan.Hotkey) + "Keyboard", IconScan.Hotkey.KeyboardKeys);
			config.WriteEnumerableEnum(nameof(IconScan.Hotkey) + "Mouse", IconScan.Hotkey.MouseButtons);
			config.WriteBool(nameof(IconScan.UseCachedIcons), IconScan.UseCachedIcons);

			config.Section = nameof(ToolTip);
			config.WriteInt(nameof(ToolTip.Duration), ToolTip.Duration);
			config.WriteString(nameof(ToolTip.DigitGroupingSymbol), ToolTip.DigitGroupingSymbol);

			config.Section = nameof(MinimalUi);
			config.WriteBool(nameof(MinimalUi.ShowName), MinimalUi.ShowName);
			config.WriteBool(nameof(MinimalUi.ShowAvgDayPrice), MinimalUi.ShowAvgDayPrice);
			config.WriteBool(nameof(MinimalUi.ShowPricePerSlot), MinimalUi.ShowPricePerSlot);
			config.WriteBool(nameof(MinimalUi.ShowTraderPrice), MinimalUi.ShowTraderPrice);
			config.WriteBool(nameof(MinimalUi.ShowTraderMaxPrice), MinimalUi.ShowTraderMaxPrice);
			config.WriteBool(nameof(MinimalUi.ShowUpdated), MinimalUi.ShowUpdated);
			config.WriteBool(nameof(MinimalUi.ShowQuestHideoutTracker), MinimalUi.ShowQuestHideoutTracker);
			config.WriteBool(nameof(MinimalUi.ShowQuestHideoutTeamTracker), MinimalUi.ShowQuestHideoutTeamTracker);
			config.WriteInt(nameof(MinimalUi.Opacity), MinimalUi.Opacity);

			config.Section = nameof(Tracking);
			config.WriteBool(nameof(Tracking.ShowNonFIRNeeds), Tracking.ShowNonFIRNeeds);

			config.Section = nameof(Tracking.TarkovTracker);
			config.WriteString(nameof(Tracking.TarkovTracker.Token), Tracking.TarkovTracker.Token);
			config.WriteBool(nameof(Tracking.TarkovTracker.ShowTeam), Tracking.TarkovTracker.ShowTeam);

			config.Section = "Other";
			config.WriteInt(nameof(ScreenResolution), (int)ScreenResolution);
			config.WriteBool(nameof(MinimizeToTray), MinimizeToTray);
			config.WriteBool(nameof(AlwaysOnTop), AlwaysOnTop);
			config.WriteString(nameof(ItemDataVersion), ItemDataVersion);
			config.WriteBool(nameof(LogDebug), LogDebug);
			config.WriteInt(nameof(ConfigVersion), ConfigVersion);
		}

		/// <summary>
		/// Converts PrimaryScreen resolution to Resolution enum, sets screenResolution if a match is found
		/// </summary>
		internal static void TrySetScreenResolution()
		{
			var boundsRectangle = Screen.PrimaryScreen.Bounds;
			var resolutionString = $"R{boundsRectangle.Width}x{boundsRectangle.Height}";
			Enum.TryParse(typeof(Resolution), resolutionString, out var matchingResolution);
			if (matchingResolution != null) screenResolution = (Resolution)matchingResolution;
		}
	}
}
