using Newtonsoft.Json.Linq;
using RatScanner.TarkovDev.Json;
using RatStash;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Key = System.Windows.Input.Key;

namespace RatScanner;

internal static partial class RatConfig {
	[StructLayout(LayoutKind.Sequential)]
	private struct NativePoint {
		public int X;
		public int Y;
	}

	[LibraryImport("user32.dll", EntryPoint = "MonitorFromPoint")]
	private static partial IntPtr MonitorFromPoint(NativePoint pt, uint dwFlags);

	[LibraryImport("Shcore.dll", EntryPoint = "GetDpiForMonitor")]
	private static partial IntPtr GetDpiForMonitor(IntPtr hmonitor, DpiType dpiType, out uint dpiX, out uint dpiY);

	// Version
	public static string Version => Process.GetCurrentProcess().MainModule?.FileVersionInfo.ProductVersion ?? "Unknown";

	public const string SINGLE_INSTANCE_GUID = "{a057bb64-c126-4ef4-a4ed-3037c2e7bc89}";

	// Paths
	internal static class Paths {
		internal static string Base = AppDomain.CurrentDomain.BaseDirectory;
		internal static string Data = Path.Combine(Base, "Data");
		internal static string StaticIcon = Path.Combine(Data, "icons");
		internal static string Locales = Path.Combine(Data, "locales");

		internal static string UserData = Path.Combine(Base, "UserData");

		private const string EftTempDir = "Battlestate Games\\EscapeFromTarkov\\";
		private static readonly string EftTemp = Path.Combine(Path.GetTempPath(), EftTempDir);
		private static readonly string TempDir = Path.Combine(Path.GetTempPath(), "RatScanner");
		internal static readonly string CacheDir = Path.Combine(TempDir, "Cache");
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

		internal static string i18nDir => Path.Combine(Base, "i18n");
	}

	// Name Scan options
	internal static class NameScan {
		internal static bool Enable = true;
		internal static bool EnableAuto = false;
		internal static Language Language = Language.English;
		internal static float ConfWarnThreshold = 0.85f;
		internal static Hotkey Hotkey = new(null, [MouseButton.Left]);
		internal static int MarkerScanSize => (int)(50 * GameScale);
		internal static int TextWidth => (int)(600 * GameScale);
	}

	// Icon Scan options
	internal static class IconScan {
		internal static bool Enable = true;
		internal static float ConfWarnThreshold = 0.8f;
		internal static bool ScanRotatedIcons = true;
		internal static int ScanWidth => (int)(GameScale * 896);
		internal static int ScanHeight => (int)(GameScale * 896);
		internal static Hotkey Hotkey = new([Key.LeftShift], [MouseButton.Left]);
		internal static bool UseCachedIcons = true;
	}

	// ToolTip options
	internal static class ToolTip {
		internal static string DigitGroupingSymbol = ".";
		internal static int Duration = 1500;
	}

	// UI options
	internal static class UserInterface {
		internal static UiLanguage Language = UiLanguage.English;
	}

	// Minimal UI
	internal static class MinimalUi {
		internal static bool ShowName = true;
		internal static bool ShowAvgDayPrice = true;
		internal static bool ShowPricePerSlot = true;
		internal static bool ShowTraderPrice = true;
		internal static bool ShowUpdated = false;
		internal static bool ShowKappa = false;
		internal static bool ShowQuestHideoutTracker = true;
		internal static bool ShowQuestHideoutTeamTracker = false;
		internal static int Opacity = 90;

		/// <summary>
		/// Smallest non-zero transparency, used when <see cref="Opacity"/> is 0 so the
		/// panel keeps a faint backdrop instead of vanishing entirely.
		/// </summary>
		internal const byte MinimumAlpha = 1;

		/// <summary>
		/// Maps the 0-100 <see cref="Opacity"/> setting onto a 0-255 backdrop alpha.
		/// </summary>
		internal static byte OpacityToAlpha(int opacity) {
			var clamped = Math.Clamp(opacity, 0, 100);
			return clamped == 0 ? MinimumAlpha : (byte)(clamped * 255 / 100);
		}
	}

	// Progress Tracking options
	internal static class Tracking {
		internal static bool ShowNonFIRNeeds = true;

		internal static bool ShowKappaNeeds = false;

		internal static ProgressSource Source = ProgressSource.Local;

		internal static class TarkovTracker {
			internal static TarkovTrackerBackend Backend = TarkovTrackerBackend.TarkovTrackerIO;
			internal static string Endpoint => Backend == TarkovTrackerBackend.TarkovTrackerIO
				? "https://tarkovtracker.io/api/v2"
				: "https://api.tarkovtracker.org";
			internal static bool Enable => Token.Length > 0;

			internal static string Token = "";
			internal static bool ShowTeam = true;
			internal static int RefreshTime = 5 * 60 * 1000; // 5 minutes
		}
	}

	public enum TarkovTrackerBackend {
		TarkovTrackerIO,
		TarkovTrackerORG,
	}

	/// <summary>Reads the game's own log files to track quests, raids and position.</summary>
	internal static class LogTracking {
		internal static bool Enable = false;
		internal static bool TrackQuests = true;
		internal static bool TrackRaidLifecycle = true;
		internal static bool TrackGameMode = true;
		internal static bool TrackPlayerPosition = true;

		/// <summary>Overrides the auto-detected logs folder when set.</summary>
		internal static string LogsPathOverride = "";

		/// <summary>Map shown for the player position when no raid was tracked.</summary>
		internal static string PositionMapFallback = "";
	}

	/// <summary>Where quest progress comes from.</summary>
	public enum ProgressSource {
		Local,
		TarkovTracker,
	}

	// Overlay options
	internal static class Overlay {
		internal static class Search {
			internal static bool Enable = true;
			internal static bool BlurBehind = true;
			internal static Hotkey Hotkey = new([Key.N, Key.M]);
			internal static Hotkey CloseHotkey = new([Key.Escape]);
		}

		internal static class Scanning {
			internal static bool Enable = true;
		}
	}

	// Application hotkeys
	internal static class Hotkeys {
		internal static Hotkey OpenWiki = new();
		internal static Hotkey OpenTarkovDev = new();
		internal static Hotkey OpenMap = new([Key.P, Key.O]);
	}

	// OAuth2 refresh tokens
	internal static class OAuthRefreshToken {
		internal static string Discord = "";
		internal static string Patreon = "";
	}

	// Other
#if DEBUG
	internal static bool LogDebug {
		get => true;
		set { }
	}
#else
	internal static bool LogDebug = false;
#endif
	internal static GameMode GameMode = GameMode.Regular;
	internal static bool MinimizeToTray = false;
	internal static bool AlwaysOnTop = true;
	internal static int SuperShortTTL = 30; // 30 seconds
	internal static int ShortTTL = 60 * 5; // 5 minutes
	internal static int MediumTTL = 60 * 60 * 1; // 1 hour
	internal static int LongTTL = 60 * 60 * 12; // 12 hours
	private static int ConfigVersion => 2;

	internal static int ScreenWidth = 1920;
	internal static int ScreenHeight = 1080;
	internal static float ScreenScale = 1f;
	internal static bool SetScreen = false;
	/// <summary>
	/// When true the user has pinned the screen resolution / scale by hand in the
	/// Advanced settings, so the auto-detected values are no longer used.
	/// </summary>
	internal static bool OverrideScreenConfig = false;
	internal static int LastWindowPositionX = int.MinValue;
	internal static int LastWindowPositionY = int.MinValue;
	internal static int LastWindowWidth = 350;
	internal static int LastWindowHeight = 500;
	internal static WindowMode LastWindowMode = WindowMode.Normal;

	internal static float GameScale => RatScannerMain.Instance.RatEyeEngine.Config.ProcessingConfig.Scale;

	private static bool IsSupportedConfigVersion() {
		SimpleConfig config = new(Paths.ConfigFile, "Other");
		var readConfigVersion = config.ReadInt(nameof(ConfigVersion), -1);
		var isSupportedConfigVersion = ConfigVersion == readConfigVersion;
		if (!isSupportedConfigVersion) Logger.LogWarning("Config version (" + readConfigVersion + ") is not supported!");
		return isSupportedConfigVersion;
	}

	internal static void LoadConfig() {
		var configFileExists = File.Exists(Paths.ConfigFile);
		var isSupportedConfigVersion = IsSupportedConfigVersion();
		if (configFileExists && !isSupportedConfigVersion) {
			var message = "Old config version detected!\n\n";
			message += "It will be removed and replaced with a new config file.\n";
			message += "Please make sure to reconfigure your settings after.";
			Logger.ShowMessage(message);

			File.Delete(Paths.ConfigFile);
			TrySetScreenConfig();
			SaveConfig();
		} else if (!configFileExists) {
			TrySetScreenConfig();
			SaveConfig();
		}

		SimpleConfig config = new(Paths.ConfigFile) {
			Section = nameof(NameScan)
		};
		NameScan.Enable = config.ReadBool(nameof(NameScan.Enable), NameScan.Enable);
		NameScan.EnableAuto = config.ReadBool(nameof(NameScan.EnableAuto), NameScan.EnableAuto);
		NameScan.Language = (Language)config.ReadInt(nameof(NameScan.Language), (int)NameScan.Language);
		NameScan.Hotkey = config.ReadHotkey(nameof(NameScan.Hotkey), NameScan.Hotkey);

		config.Section = nameof(IconScan);
		IconScan.Enable = config.ReadBool(nameof(IconScan.Enable), IconScan.Enable);
		IconScan.ScanRotatedIcons = config.ReadBool(nameof(IconScan.ScanRotatedIcons), IconScan.ScanRotatedIcons);
		IconScan.Hotkey = config.ReadHotkey(nameof(IconScan.Hotkey), IconScan.Hotkey);
		IconScan.UseCachedIcons = config.ReadBool(nameof(IconScan.UseCachedIcons), IconScan.UseCachedIcons);

		config.Section = nameof(ToolTip);
		ToolTip.Duration = config.ReadInt(nameof(ToolTip.Duration), ToolTip.Duration);
		ToolTip.DigitGroupingSymbol = config.ReadString(nameof(ToolTip.DigitGroupingSymbol), ToolTip.DigitGroupingSymbol);

		config.Section = nameof(UserInterface);
		UserInterface.Language = (UiLanguage)config.ReadInt(nameof(UserInterface.Language), (int)UserInterface.Language);

		config.Section = nameof(MinimalUi);
		MinimalUi.ShowName = config.ReadBool(nameof(MinimalUi.ShowName), MinimalUi.ShowName);
		MinimalUi.ShowAvgDayPrice = config.ReadBool(nameof(MinimalUi.ShowAvgDayPrice), MinimalUi.ShowAvgDayPrice);
		MinimalUi.ShowPricePerSlot = config.ReadBool(nameof(MinimalUi.ShowPricePerSlot), MinimalUi.ShowPricePerSlot);
		MinimalUi.ShowTraderPrice = config.ReadBool(nameof(MinimalUi.ShowTraderPrice), MinimalUi.ShowTraderPrice);
		MinimalUi.ShowUpdated = config.ReadBool(nameof(MinimalUi.ShowUpdated), MinimalUi.ShowUpdated);
		MinimalUi.ShowKappa = config.ReadBool(nameof(MinimalUi.ShowKappa), MinimalUi.ShowKappa);
		MinimalUi.ShowQuestHideoutTracker = config.ReadBool(nameof(MinimalUi.ShowQuestHideoutTracker), MinimalUi.ShowQuestHideoutTracker);
		MinimalUi.ShowQuestHideoutTeamTracker = config.ReadBool(nameof(MinimalUi.ShowQuestHideoutTeamTracker), MinimalUi.ShowQuestHideoutTeamTracker);
		MinimalUi.Opacity = config.ReadInt(nameof(MinimalUi.Opacity), MinimalUi.Opacity);

		config.Section = nameof(Tracking);
		Tracking.Source = (ProgressSource)config.ReadInt(nameof(Tracking.Source), (int)Tracking.Source);
		Tracking.ShowNonFIRNeeds = config.ReadBool(nameof(Tracking.ShowNonFIRNeeds), Tracking.ShowNonFIRNeeds);
		Tracking.ShowKappaNeeds = config.ReadBool(nameof(Tracking.ShowKappaNeeds), Tracking.ShowKappaNeeds);

		config.Section = nameof(Tracking.TarkovTracker);
		Tracking.TarkovTracker.Backend = (TarkovTrackerBackend)config.ReadInt(nameof(Tracking.TarkovTracker.Backend), (int)Tracking.TarkovTracker.Backend);
		Tracking.TarkovTracker.Token = config.ReadSecureString(nameof(Tracking.TarkovTracker.Token), Tracking.TarkovTracker.Token);
		Tracking.TarkovTracker.ShowTeam = config.ReadBool(nameof(Tracking.TarkovTracker.ShowTeam), Tracking.TarkovTracker.ShowTeam);

		config.Section = nameof(LogTracking);
		LogTracking.Enable = config.ReadBool(nameof(LogTracking.Enable), LogTracking.Enable);
		LogTracking.TrackQuests = config.ReadBool(nameof(LogTracking.TrackQuests), LogTracking.TrackQuests);
		LogTracking.TrackRaidLifecycle = config.ReadBool(nameof(LogTracking.TrackRaidLifecycle), LogTracking.TrackRaidLifecycle);
		LogTracking.TrackGameMode = config.ReadBool(nameof(LogTracking.TrackGameMode), LogTracking.TrackGameMode);
		LogTracking.TrackPlayerPosition = config.ReadBool(nameof(LogTracking.TrackPlayerPosition), LogTracking.TrackPlayerPosition);
		LogTracking.LogsPathOverride = config.ReadString(nameof(LogTracking.LogsPathOverride), LogTracking.LogsPathOverride);
		LogTracking.PositionMapFallback = config.ReadString(nameof(LogTracking.PositionMapFallback), LogTracking.PositionMapFallback);

		config.Section = nameof(Overlay);

		config.Section = nameof(Overlay.Search);
		Overlay.Search.Enable = config.ReadBool(nameof(Overlay.Search.Enable), Overlay.Search.Enable);
		Overlay.Search.BlurBehind = config.ReadBool(nameof(Overlay.Search.BlurBehind), Overlay.Search.BlurBehind);
		Overlay.Search.Hotkey = config.ReadHotkey(nameof(Overlay.Search.Hotkey), Overlay.Search.Hotkey);
		Overlay.Search.CloseHotkey = config.ReadHotkey(nameof(Overlay.Search.CloseHotkey), Overlay.Search.CloseHotkey);

		config.Section = nameof(Overlay.Scanning);
		Overlay.Scanning.Enable = config.ReadBool(nameof(Overlay.Scanning.Enable), Overlay.Scanning.Enable);

		config.Section = nameof(Hotkeys);
		Hotkeys.OpenWiki = config.ReadHotkey(nameof(Hotkeys.OpenWiki), Hotkeys.OpenWiki);
		Hotkeys.OpenTarkovDev = config.ReadHotkey(nameof(Hotkeys.OpenTarkovDev), Hotkeys.OpenTarkovDev);
		Hotkeys.OpenMap = config.ReadHotkey(nameof(Hotkeys.OpenMap), Hotkeys.OpenMap);

		config.Section = nameof(OAuthRefreshToken);
		OAuthRefreshToken.Discord = config.ReadSecureString(nameof(OAuthRefreshToken.Discord), OAuthRefreshToken.Discord);
		OAuthRefreshToken.Patreon = config.ReadSecureString(nameof(OAuthRefreshToken.Patreon), OAuthRefreshToken.Patreon);

		config.Section = "Other";
		OverrideScreenConfig = config.ReadBool(nameof(OverrideScreenConfig), OverrideScreenConfig);
		if (!SetScreen || OverrideScreenConfig) {
			ScreenWidth = config.ReadInt(nameof(ScreenWidth), ScreenWidth);
			ScreenHeight = config.ReadInt(nameof(ScreenHeight), ScreenHeight);
			ScreenScale = config.ReadFloat(nameof(ScreenScale), ScreenScale);
		}

		GameMode = (GameMode)config.ReadInt(nameof(GameMode), (int)GameMode);
		MinimizeToTray = config.ReadBool(nameof(MinimizeToTray), MinimizeToTray);
		AlwaysOnTop = config.ReadBool(nameof(AlwaysOnTop), AlwaysOnTop);
		LogDebug = config.ReadBool(nameof(LogDebug), LogDebug);

		LastWindowPositionX = config.ReadInt(nameof(LastWindowPositionX), LastWindowPositionX);
		LastWindowPositionY = config.ReadInt(nameof(LastWindowPositionY), LastWindowPositionY);
		LastWindowWidth = config.ReadInt(nameof(LastWindowWidth), LastWindowWidth);
		LastWindowHeight = config.ReadInt(nameof(LastWindowHeight), LastWindowHeight);
		LastWindowMode = (WindowMode)config.ReadInt(nameof(LastWindowMode), (int)LastWindowMode);
	}

	internal static void SaveConfig() {
		SimpleConfig config = new(Paths.ConfigFile) {
			Section = nameof(NameScan)
		};
		config.WriteBool(nameof(NameScan.Enable), NameScan.Enable);
		config.WriteBool(nameof(NameScan.EnableAuto), NameScan.EnableAuto);
		config.WriteInt(nameof(NameScan.Language), (int)NameScan.Language);
		config.WriteHotkey(nameof(NameScan.Hotkey), NameScan.Hotkey);

		config.Section = nameof(IconScan);
		config.WriteBool(nameof(IconScan.Enable), IconScan.Enable);
		config.WriteBool(nameof(IconScan.ScanRotatedIcons), IconScan.ScanRotatedIcons);
		config.WriteHotkey(nameof(IconScan.Hotkey), IconScan.Hotkey);
		config.WriteBool(nameof(IconScan.UseCachedIcons), IconScan.UseCachedIcons);

		config.Section = nameof(ToolTip);
		config.WriteInt(nameof(ToolTip.Duration), ToolTip.Duration);
		config.WriteString(nameof(ToolTip.DigitGroupingSymbol), ToolTip.DigitGroupingSymbol);

		config.Section = nameof(UserInterface);
		config.WriteInt(nameof(UserInterface.Language), (int)UserInterface.Language);

		config.Section = nameof(MinimalUi);
		config.WriteBool(nameof(MinimalUi.ShowName), MinimalUi.ShowName);
		config.WriteBool(nameof(MinimalUi.ShowAvgDayPrice), MinimalUi.ShowAvgDayPrice);
		config.WriteBool(nameof(MinimalUi.ShowPricePerSlot), MinimalUi.ShowPricePerSlot);
		config.WriteBool(nameof(MinimalUi.ShowTraderPrice), MinimalUi.ShowTraderPrice);
		config.WriteBool(nameof(MinimalUi.ShowUpdated), MinimalUi.ShowUpdated);
		config.WriteBool(nameof(MinimalUi.ShowKappa), MinimalUi.ShowKappa);
		config.WriteBool(nameof(MinimalUi.ShowQuestHideoutTracker), MinimalUi.ShowQuestHideoutTracker);
		config.WriteBool(nameof(MinimalUi.ShowQuestHideoutTeamTracker), MinimalUi.ShowQuestHideoutTeamTracker);
		config.WriteInt(nameof(MinimalUi.Opacity), MinimalUi.Opacity);

		config.Section = nameof(Tracking);
		config.WriteInt(nameof(Tracking.Source), (int)Tracking.Source);
		config.WriteBool(nameof(Tracking.ShowNonFIRNeeds), Tracking.ShowNonFIRNeeds);
		config.WriteBool(nameof(Tracking.ShowKappaNeeds), Tracking.ShowKappaNeeds);

		config.Section = nameof(Tracking.TarkovTracker);
		config.WriteInt(nameof(Tracking.TarkovTracker.Backend), (int)Tracking.TarkovTracker.Backend);
		config.WriteSecureString(nameof(Tracking.TarkovTracker.Token), Tracking.TarkovTracker.Token);
		config.WriteBool(nameof(Tracking.TarkovTracker.ShowTeam), Tracking.TarkovTracker.ShowTeam);

		config.Section = nameof(LogTracking);
		config.WriteBool(nameof(LogTracking.Enable), LogTracking.Enable);
		config.WriteBool(nameof(LogTracking.TrackQuests), LogTracking.TrackQuests);
		config.WriteBool(nameof(LogTracking.TrackRaidLifecycle), LogTracking.TrackRaidLifecycle);
		config.WriteBool(nameof(LogTracking.TrackGameMode), LogTracking.TrackGameMode);
		config.WriteBool(nameof(LogTracking.TrackPlayerPosition), LogTracking.TrackPlayerPosition);
		config.WriteString(nameof(LogTracking.LogsPathOverride), LogTracking.LogsPathOverride);
		config.WriteString(nameof(LogTracking.PositionMapFallback), LogTracking.PositionMapFallback);

		config.Section = nameof(Overlay);

		config.Section = nameof(Overlay.Search);
		config.WriteBool(nameof(Overlay.Search.Enable), Overlay.Search.Enable);
		config.WriteBool(nameof(Overlay.Search.BlurBehind), Overlay.Search.BlurBehind);
		config.WriteHotkey(nameof(Overlay.Search.Hotkey), Overlay.Search.Hotkey);
		config.WriteHotkey(nameof(Overlay.Search.CloseHotkey), Overlay.Search.CloseHotkey);

		config.Section = nameof(Overlay.Scanning);
		config.WriteBool(nameof(Overlay.Scanning.Enable), Overlay.Scanning.Enable);

		config.Section = nameof(Hotkeys);
		config.WriteHotkey(nameof(Hotkeys.OpenWiki), Hotkeys.OpenWiki);
		config.WriteHotkey(nameof(Hotkeys.OpenTarkovDev), Hotkeys.OpenTarkovDev);
		config.WriteHotkey(nameof(Hotkeys.OpenMap), Hotkeys.OpenMap);

		config.Section = nameof(OAuthRefreshToken);
		config.WriteSecureString(nameof(OAuthRefreshToken.Discord), OAuthRefreshToken.Discord);
		config.WriteSecureString(nameof(OAuthRefreshToken.Patreon), OAuthRefreshToken.Patreon);

		config.Section = "Other";
		config.WriteInt(nameof(ScreenWidth), ScreenWidth);
		config.WriteInt(nameof(ScreenHeight), ScreenHeight);
		config.WriteFloat(nameof(ScreenScale), ScreenScale);
		config.WriteBool(nameof(OverrideScreenConfig), OverrideScreenConfig);
		config.WriteInt(nameof(GameMode), (int)GameMode);
		config.WriteBool(nameof(MinimizeToTray), MinimizeToTray);
		config.WriteBool(nameof(AlwaysOnTop), AlwaysOnTop);
		config.WriteBool(nameof(LogDebug), LogDebug);
		config.WriteInt(nameof(ConfigVersion), ConfigVersion);
		config.WriteInt(nameof(LastWindowPositionX), LastWindowPositionX);
		config.WriteInt(nameof(LastWindowPositionY), LastWindowPositionY);
		config.WriteInt(nameof(LastWindowWidth), LastWindowWidth);
		config.WriteInt(nameof(LastWindowHeight), LastWindowHeight);
		config.WriteInt(nameof(LastWindowMode), (int)LastWindowMode);
	}

	internal static bool ReadFromCache(string key, out string value) {
		var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
		var hash = string.Concat(Array.ConvertAll(hashBytes, b => b.ToString("X2")));

		var path = Path.Combine(Paths.CacheDir, hash + ".data");
		value = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
		return value != string.Empty;
	}

	internal static void WriteToCache(string key, string value) {
		var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
		var hash = string.Concat(Array.ConvertAll(hashBytes, b => b.ToString("X2")));

		var path = Path.Combine(Paths.CacheDir, hash + ".data");
		_ = Directory.CreateDirectory(Paths.CacheDir);
		File.WriteAllText(path, value);
	}

	/// <summary>
	/// Get the current screen config from tarkov's config files or default to the primary screen
	/// </summary>
	internal static void TrySetScreenConfig() {
		(var width, var height, var scale) = GetTarkovScreenConfig();
		ScreenWidth = width;
		ScreenHeight = height;
		ScreenScale = (float)scale;
		SetScreen = true;
	}

	public enum DpiType {
		Effective = 0,
		Angular = 1,
		Raw = 2,
	}

	public enum WindowMode {
		Normal = 0,
		Minimal = 1,
		Minimized = 2,
	}

	public static double GetScalingForScreen(Screen screen) {
		NativePoint pointOnScreen = new() { X = screen.Bounds.X + 1, Y = screen.Bounds.Y + 1 };
		var mon = MonitorFromPoint(pointOnScreen, 2 /*MONITOR_DEFAULTTONEAREST*/);
		_ = GetDpiForMonitor(mon, DpiType.Effective, out var dpiX, out _);
		return dpiX / 96.0;
	}

	private static (int widht, int height, double scale) GetTarkovScreenConfig() {
		try {
			var configPath = Environment.ExpandEnvironmentVariables(@"%AppData%\Battlestate Games\Escape From Tarkov\Settings\Graphics.ini");
			using FileStream file = new(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using StreamReader reader = new(file, Encoding.UTF8);

			var json = JObject.Parse(reader.ReadToEnd());

			var activeDisplay = json["DisplaySettings"]["Display"].ToObject<int>();
			var windowRes = json["Stored"][activeDisplay.ToString()]["WindowResolution"];
			var width = windowRes["Width"].ToObject<int>();
			var height = windowRes["Height"].ToObject<int>();

			var usedScreen = Screen.AllScreens[activeDisplay];
			var scale = GetScalingForScreen(usedScreen);

			return (width, height, scale);
		} catch (Exception e) {
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
