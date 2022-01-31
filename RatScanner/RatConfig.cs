using RatScanner.Controls;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using RatStash;
using Key = System.Windows.Input.Key;

namespace RatScanner;

internal static class RatConfig
{
	// Version
	public static string Version => Process.GetCurrentProcess().MainModule.FileVersionInfo.ProductVersion;

	// Paths
	internal static class Paths
	{
		internal static string Base = AppDomain.CurrentDomain.BaseDirectory;
		internal static string Data = Path.Combine(Base, "Data");
		internal static string StaticIcon = Path.Combine(Data, "icons");
		internal static string Locales = Path.Combine(Data, "locales");

		private const string EftTempDir = "Battlestate Games\\EscapeFromTarkov\\";
		private static readonly string EftTemp = Path.Combine(Path.GetTempPath(), EftTempDir);
		internal static string DynamicIcon = Path.Combine(EftTemp, "Icon Cache");
		internal static string StaticCorrelation = Path.Combine(StaticIcon, "correlation.json");
		internal static string DynamicCorrelation = Path.Combine(DynamicIcon, "index.json");
		internal static string ItemData = Path.Combine(Data, "items.json");
		internal static string TrainedData = Path.Combine(Data, "traineddata");
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
		internal static Language Language = Language.English;
		internal static float ConfWarnThreshold = 0.85f;
		internal static int MarkerScanSize => (int)(50 * ScreenScale);
		internal static int TextWidth => (int)(600 * ScreenScale);
	}

	// Icon Scan options
	internal static class IconScan
	{
		internal static bool Enable = true;
		internal static float ConfWarnThreshold = 0.8f;
		internal static bool ScanRotatedIcons = true;
		internal static int ScanWidth => (int)(ScreenScale * 896);
		internal static int ScanHeight => (int)(ScreenScale * 896);
		internal static Hotkey Hotkey = new(new[] { Key.LeftShift }.ToList(), new[] { MouseButton.Left });
		internal static bool UseCachedIcons = true;
	}

	// ToolTip options
	internal static class ToolTip
	{
		internal static string DigitGroupingSymbol = ".";
		internal static int Duration = 1500;
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
			internal static int RefreshTime = 5 * 60 * 1000; // 5 minutes
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
	internal static int MarketDBRefreshTime = 30 * 60 * 1000; // 30 minutes
	internal static string ItemDataBundleVersion = "20220118";
	private static int ConfigVersion => 2;

	internal static int ScreenWidth = 1920;
	internal static int ScreenHeight = 1080;
	internal static bool SetScreen = false;

	internal static float ScreenScale => RatScannerMain.Instance.RatEyeConfig.ProcessingConfig.Scale;

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
		NameScan.Language = (Language)config.ReadInt(nameof(NameScan.Language), (int)Language.English);

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
		if (!SetScreen)
		{
			ScreenWidth = config.ReadInt(nameof(ScreenWidth), 1920);
			ScreenHeight = config.ReadInt(nameof(ScreenHeight), 1080);
		}

		MinimizeToTray = config.ReadBool(nameof(MinimizeToTray), false);
		AlwaysOnTop = config.ReadBool(nameof(AlwaysOnTop), false);
		ItemDataBundleVersion = config.ReadString(nameof(ItemDataBundleVersion), "20220118");
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
		config.WriteInt(nameof(ScreenWidth), ScreenWidth);
		config.WriteInt(nameof(ScreenHeight), ScreenHeight);
		config.WriteBool(nameof(MinimizeToTray), MinimizeToTray);
		config.WriteBool(nameof(AlwaysOnTop), AlwaysOnTop);
		config.WriteString(nameof(ItemDataBundleVersion), ItemDataBundleVersion);
		config.WriteBool(nameof(LogDebug), LogDebug);
		config.WriteInt(nameof(ConfigVersion), ConfigVersion);
	}

	/// <summary>
	/// Converts PrimaryScreen resolution to Resolution enum, sets screenResolution if a match is found
	/// </summary>
	internal static void TrySetScreenResolution()
	{
		ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
		ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
		SetScreen = true;
		var message = $"Detected {ScreenWidth}x{ScreenHeight} Resolution.\n\n";
		message += "You can adjust this inside the settings.";
		Logger.ShowMessage(message);
	}
}
