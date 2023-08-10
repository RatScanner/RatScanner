using Newtonsoft.Json.Linq;
using RatScanner.Controls;
using RatStash;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Key = System.Windows.Input.Key;

namespace RatScanner;

internal static class RatConfig
{
	[DllImport("user32.dll")]
	private static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

	[DllImport("Shcore.dll")]
	private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

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
		internal static bool EnableAuto = false;
		internal static Language Language = Language.English;
		internal static float ConfWarnThreshold = 0.85f;
		internal static int MarkerScanSize => (int)(50 * GameScale);
		internal static int TextWidth => (int)(600 * GameScale);
	}

	// Icon Scan options
	internal static class IconScan
	{
		internal static bool Enable = true;
		internal static float ConfWarnThreshold = 0.8f;
		internal static bool ScanRotatedIcons = true;
		internal static int ScanWidth => (int)(GameScale * 896);
		internal static int ScanHeight => (int)(GameScale * 896);
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
		internal static bool ShowUpdated = false;
		internal static bool ShowQuestHideoutTracker = true;
		internal static bool ShowQuestHideoutTeamTracker = false;
		internal static int Opacity = 0;
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

	// Overlay options
	internal static class Overlay
	{
		internal static class Search
		{
			internal static bool Enable = true;
			internal static Hotkey Hotkey = new(new[] { Key.N, Key.M }.ToList());
		}
	}

	internal static class TarkovTools
	{
		internal static class RemoteControl
		{
			internal static bool Enable => !string.IsNullOrEmpty(SessionId);

			internal static string SessionId = string.Empty;
			internal static bool AutoSync = false;
			internal static bool OpenAmmoChart = false;
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
	private static int ConfigVersion => 2;

	internal static int ScreenWidth = 1920;
	internal static int ScreenHeight = 1080;
	internal static float ScreenScale = 1f;
	internal static bool SetScreen = false;

	internal static float GameScale => RatScannerMain.Instance.RatEyeEngine.Config.ProcessingConfig.Scale;

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
			TrySetScreenConfig();
			SaveConfig();
		}
		else if (!configFileExists)
		{
			TrySetScreenConfig();
			SaveConfig();
		}

		var config = new SimpleConfig(Paths.ConfigFile);

		config.Section = nameof(NameScan);
		NameScan.Enable = config.ReadBool(nameof(NameScan.Enable), NameScan.Enable);
		NameScan.EnableAuto = config.ReadBool(nameof(NameScan.EnableAuto), NameScan.EnableAuto);
		NameScan.Language = (Language)config.ReadInt(nameof(NameScan.Language));

		config.Section = nameof(IconScan);
		IconScan.Enable = config.ReadBool(nameof(IconScan.Enable), IconScan.Enable);
		IconScan.ScanRotatedIcons = config.ReadBool(nameof(IconScan.ScanRotatedIcons), IconScan.ScanRotatedIcons);
		var keyboardKeys = config.ReadEnumerableEnum(nameof(IconScan.Hotkey) + "Keyboard", IconScan.Hotkey.KeyboardKeys);
		var mouseButtons = config.ReadEnumerableEnum(nameof(IconScan.Hotkey) + "Mouse", IconScan.Hotkey.MouseButtons);
		IconScan.Hotkey = new Hotkey(keyboardKeys.ToList(), mouseButtons.ToList());
		IconScan.UseCachedIcons = config.ReadBool(nameof(IconScan.UseCachedIcons), IconScan.UseCachedIcons);

		config.Section = nameof(ToolTip);
		ToolTip.Duration = config.ReadInt(nameof(ToolTip.Duration), ToolTip.Duration);
		ToolTip.DigitGroupingSymbol = config.ReadString(nameof(ToolTip.DigitGroupingSymbol), ToolTip.DigitGroupingSymbol);

		config.Section = nameof(MinimalUi);
		MinimalUi.ShowName = config.ReadBool(nameof(MinimalUi.ShowName), MinimalUi.ShowName);
		MinimalUi.ShowAvgDayPrice = config.ReadBool(nameof(MinimalUi.ShowAvgDayPrice), MinimalUi.ShowAvgDayPrice);
		MinimalUi.ShowPricePerSlot = config.ReadBool(nameof(MinimalUi.ShowPricePerSlot), MinimalUi.ShowPricePerSlot);
		MinimalUi.ShowTraderPrice = config.ReadBool(nameof(MinimalUi.ShowTraderPrice), MinimalUi.ShowTraderPrice);
		MinimalUi.ShowUpdated = config.ReadBool(nameof(MinimalUi.ShowUpdated), MinimalUi.ShowUpdated);
		MinimalUi.ShowQuestHideoutTracker = config.ReadBool(nameof(MinimalUi.ShowQuestHideoutTracker), MinimalUi.ShowQuestHideoutTracker);
		MinimalUi.ShowQuestHideoutTeamTracker = config.ReadBool(nameof(MinimalUi.ShowQuestHideoutTeamTracker), MinimalUi.ShowQuestHideoutTeamTracker);
		MinimalUi.Opacity = config.ReadInt(nameof(MinimalUi.Opacity), MinimalUi.Opacity);

		config.Section = nameof(Tracking);
		Tracking.ShowNonFIRNeeds = config.ReadBool(nameof(Tracking.ShowNonFIRNeeds), Tracking.ShowNonFIRNeeds);

		config.Section = nameof(Tracking.TarkovTracker);
		Tracking.TarkovTracker.Token = config.ReadString(nameof(Tracking.TarkovTracker.Token), Tracking.TarkovTracker.Token);
		Tracking.TarkovTracker.ShowTeam = config.ReadBool(nameof(Tracking.TarkovTracker.ShowTeam), Tracking.TarkovTracker.ShowTeam);

		config.Section = nameof(Overlay);

		config.Section = nameof(Overlay.Search);
		Overlay.Search.Enable = config.ReadBool(nameof(Overlay.Search.Enable), Overlay.Search.Enable);
		keyboardKeys = config.ReadEnumerableEnum(nameof(Overlay.Search.Hotkey) + "Keyboard", Overlay.Search.Hotkey.KeyboardKeys);
		mouseButtons = config.ReadEnumerableEnum(nameof(Overlay.Search.Hotkey) + "Mouse", Overlay.Search.Hotkey.MouseButtons);
		Overlay.Search.Hotkey = new Hotkey(keyboardKeys.ToList(), mouseButtons.ToList());

		config.Section = nameof(TarkovTools.RemoteControl);
		TarkovTools.RemoteControl.SessionId = config.ReadString(nameof(TarkovTools.RemoteControl.SessionId));
		TarkovTools.RemoteControl.AutoSync = config.ReadBool(nameof(TarkovTools.RemoteControl.AutoSync));
		TarkovTools.RemoteControl.OpenAmmoChart = config.ReadBool(nameof(TarkovTools.RemoteControl.OpenAmmoChart));

		config.Section = "Other";
		if (!SetScreen)
		{
			ScreenWidth = config.ReadInt(nameof(ScreenWidth), ScreenWidth);
			ScreenHeight = config.ReadInt(nameof(ScreenHeight), ScreenHeight);
			ScreenScale = config.ReadFloat(nameof(ScreenScale), ScreenScale);
		}

		MinimizeToTray = config.ReadBool(nameof(MinimizeToTray), MinimizeToTray);
		AlwaysOnTop = config.ReadBool(nameof(AlwaysOnTop), AlwaysOnTop);
		LogDebug = config.ReadBool(nameof(LogDebug), LogDebug);
	}

	internal static void SaveConfig()
	{
		var config = new SimpleConfig(Paths.ConfigFile);

		config.Section = nameof(NameScan);
		config.WriteBool(nameof(NameScan.Enable), NameScan.Enable);
		config.WriteBool(nameof(NameScan.EnableAuto), NameScan.EnableAuto);
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
		config.WriteBool(nameof(MinimalUi.ShowUpdated), MinimalUi.ShowUpdated);
		config.WriteBool(nameof(MinimalUi.ShowQuestHideoutTracker), MinimalUi.ShowQuestHideoutTracker);
		config.WriteBool(nameof(MinimalUi.ShowQuestHideoutTeamTracker), MinimalUi.ShowQuestHideoutTeamTracker);
		config.WriteInt(nameof(MinimalUi.Opacity), MinimalUi.Opacity);

		config.Section = nameof(Tracking);
		config.WriteBool(nameof(Tracking.ShowNonFIRNeeds), Tracking.ShowNonFIRNeeds);

		config.Section = nameof(Tracking.TarkovTracker);
		config.WriteString(nameof(Tracking.TarkovTracker.Token), Tracking.TarkovTracker.Token);
		config.WriteBool(nameof(Tracking.TarkovTracker.ShowTeam), Tracking.TarkovTracker.ShowTeam);

		config.Section = nameof(TarkovTools.RemoteControl);
		config.WriteString(nameof(TarkovTools.RemoteControl.SessionId), TarkovTools.RemoteControl.SessionId);
		config.WriteBool(nameof(TarkovTools.RemoteControl.AutoSync), TarkovTools.RemoteControl.AutoSync);
		config.WriteBool(nameof(TarkovTools.RemoteControl.OpenAmmoChart), TarkovTools.RemoteControl.OpenAmmoChart);

		config.Section = nameof(Overlay);

		config.Section = nameof(Overlay.Search);
		config.WriteBool(nameof(Overlay.Search.Enable), Overlay.Search.Enable);

		config.Section = "Other";
		config.WriteInt(nameof(ScreenWidth), ScreenWidth);
		config.WriteInt(nameof(ScreenHeight), ScreenHeight);
		config.WriteFloat(nameof(ScreenScale), ScreenScale);
		config.WriteBool(nameof(MinimizeToTray), MinimizeToTray);
		config.WriteBool(nameof(AlwaysOnTop), AlwaysOnTop);
		config.WriteBool(nameof(LogDebug), LogDebug);
		config.WriteInt(nameof(ConfigVersion), ConfigVersion);
	}

	/// <summary>
	/// Get the current screen config from tarkov's config files or default to the primary screen
	/// </summary>
	internal static void TrySetScreenConfig()
	{
		var (width, height, scale) = GetTarkovScreenConfig();
		ScreenWidth = width;
		ScreenHeight = height;
		ScreenScale = (float)scale;
		SetScreen = true;
	}

	public enum DpiType
	{
		Effective = 0,
		Angular = 1,
		Raw = 2,
	}

	public static double GetScalingForScreen(Screen screen)
	{
		var pointOnScreen = new Point(screen.Bounds.X + 1, screen.Bounds.Y + 1);
		var mon = MonitorFromPoint(pointOnScreen, 2 /*MONITOR_DEFAULTTONEAREST*/);
		GetDpiForMonitor(mon, DpiType.Effective, out var dpiX, out _);
		return dpiX / 96.0;
	}

	private static (int widht, int height, double scale) GetTarkovScreenConfig()
	{
		try
		{
			var configPath = Environment.ExpandEnvironmentVariables(@"%AppData%\Battlestate Games\Escape From Tarkov\Settings\Graphics.ini");
			using var file = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var reader = new StreamReader(file, Encoding.UTF8);

			var json = JObject.Parse(reader.ReadToEnd());

			var activeDisplay = json["DisplaySettings"]["Display"].ToObject<int>();
			var windowRes = json["Stored"][activeDisplay.ToString()]["WindowResolution"];
			var width = windowRes["Width"].ToObject<int>();
			var height = windowRes["Height"].ToObject<int>();

			var usedScreen = Screen.AllScreens[activeDisplay];
			var scale = GetScalingForScreen(usedScreen);

			return (width, height, scale);
		}
		catch (Exception e)
		{
			Logger.LogWarning("Unable to query Escape From Tarkov graphic settings.", e);

			var width = Screen.PrimaryScreen.Bounds.Width;
			var height = Screen.PrimaryScreen.Bounds.Height;
			var scale = GetScalingForScreen(Screen.PrimaryScreen);

			var message = $"Detected {width}x{height} Resolution at {scale} Scale.\n\n";
			message += "You can adjust this inside the settings.";
			Logger.ShowMessage(message);

			return (width, height, scale);
		}
	}
}
