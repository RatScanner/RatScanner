using RatScanner;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace RatScanner.Localization;

public enum UiLanguage {
	English = 0,
	Spanish = 1,
	French = 2,
	Polish = 3,
	Portuguese = 4,
	Russian = 5,
	Chinese = 6,
}

public class LocalizationService {
	private const string TranslationDirectory = "Localization/Translations";

	private static readonly IReadOnlyDictionary<UiLanguage, string> TranslationFileMap = new Dictionary<UiLanguage, string> {
		[UiLanguage.Spanish] = "es.json",
		[UiLanguage.French] = "fr.json",
		[UiLanguage.Polish] = "pl.json",
		[UiLanguage.Portuguese] = "pt.json",
		[UiLanguage.Russian] = "ru.json",
		[UiLanguage.Chinese] = "zh.json",
	};

	private static readonly IReadOnlyDictionary<UiLanguage, string> CultureMap = new Dictionary<UiLanguage, string> {
		[UiLanguage.English] = "en",
		[UiLanguage.Spanish] = "es",
		[UiLanguage.French] = "fr",
		[UiLanguage.Polish] = "pl",
		[UiLanguage.Portuguese] = "pt",
		[UiLanguage.Russian] = "ru",
		[UiLanguage.Chinese] = "zh-Hans",
	};

	private static readonly IReadOnlyDictionary<UiLanguage, string> LanguageDisplayNames = new Dictionary<UiLanguage, string> {
		[UiLanguage.English] = "English",
		[UiLanguage.Spanish] = "Español",
		[UiLanguage.French] = "Français",
		[UiLanguage.Polish] = "Polski",
		[UiLanguage.Portuguese] = "Português",
		[UiLanguage.Russian] = "Русский",
		[UiLanguage.Chinese] = "中文",
	};

	private static readonly IReadOnlyDictionary<UiLanguage, IReadOnlyDictionary<string, string>> Translations = BuildTranslations();

	private static IReadOnlyDictionary<UiLanguage, IReadOnlyDictionary<string, string>> BuildTranslations() {
		Dictionary<UiLanguage, IReadOnlyDictionary<string, string>> translations = new() {
			[UiLanguage.English] = new Dictionary<string, string> {
				["DashboardNav"] = "Dashboard",
				["SettingsNavGroup"] = "Settings",
				["GeneralNav"] = "General",
				["ScanningNav"] = "Scanning",
				["TrackingNav"] = "Tracking",
				["MinimalNav"] = "Minimal UI",
				["OverlayNav"] = "Overlay",
				["SaveButton"] = "Save",
				["CancelButton"] = "Cancel",
				["GeneralSettingsTitle"] = "General Settings",
				["TooltipDuration"] = "Tooltip Duration: {0}",
				["ScreenResolution"] = "Screen Resolution",
				["WidthLabel"] = "Width",
				["HeightLabel"] = "Height",
				["ScaleLabel"] = "Scale",
				["UsePveData"] = "Use PVE data",
				["AlwaysOnTop"] = "Always on top",
				["LogDebugInfo"] = "Log debug info",
				["InterfaceLanguage"] = "Interface Language",
				["ScanningSettingsTitle"] = "Scanning Settings",
				["NameScanLanguage"] = "Name Scan Language",
				["EnableNameScan"] = "Enable Name Scan",
				["AutomaticScanning"] = "Automatic Scanning",
				["EnableIconScan"] = "Enable Icon Scan",
				["ScanRotatedIcons"] = "Scan Rotated Icons",
				["UseCachedIcons"] = "Use Cached Icons",
				["IconScanHotkey"] = "Icon Scan Hotkey",
				["OverlaySettingsTitle"] = "Overlay Settings",
				["EnableSearchOverlay"] = "Enable Search Overlay",
				["BlurBehindSearch"] = "Blur Behind Search",
				["OpenSearchOverlay"] = "Open Search Overlay",
				["TrackingSettingsTitle"] = "Tracking Settings",
				["ShowNonFirNeeds"] = "Show Non-FIR Needs",
				["ShowKappaNeeds"] = "Show Kappa Needs",
				["BackendLabel"] = "Backend",
				["ApiToken"] = "API Token",
				["ShowTeammates"] = "Show Teammates",
				["InvalidToken"] = "Invalid token",
				["TarkovTrackerTitle"] = "TarkovTracker",
				["MinimalUiSettingsTitle"] = "Minimal UI Settings",
				["NameLabel"] = "Name",
				["AvgDailyPrice"] = "Avg. Daily Price",
				["PricePerSlot"] = "Price per Slot",
				["TraderPrice"] = "Trader Price",
				["KappaNeeded"] = "Kappa Needed",
				["NeededQuestHideout"] = "Needed Quest & Hideout",
				["NeededQuestHideoutTeam"] = "Needed Quest & Hideout Team",
				["UpdatedTimestamp"] = "Updated Timestamp",
				["OpacityLabel"] = "Opacity: {0}",
				["RecentAvgPrice"] = "Recent Avg. Price",
				["ValuePerSlot"] = "Value per Slot",
				["BestTrader"] = "Best Trader",
				["QuestLabel"] = "Quest",
				["HideoutLabel"] = "Hideout",
				["ItemSearch"] = "Item Search",
				["OpenWiki"] = "Open Wiki",
				["OpenTarkovDev"] = "Open Tarkov.dev",
				["FavoriteItem"] = "Favorite item",
				["AddItem"] = "Add item",
				["RemoveItem"] = "Remove item",
				["NoneLabel"] = "None",
			},
		};

		foreach (var pair in TranslationFileMap) {
			string filePath = Path.Combine(AppContext.BaseDirectory, TranslationDirectory, pair.Value);
			translations[pair.Key] = TranslationLoader.TryLoad(filePath, out var loadedTranslations)
				? loadedTranslations
				: translations[UiLanguage.English];
		}

		return translations;
	}

	private UiLanguage _currentLanguage;

	public LocalizationService() {
		_currentLanguage = RatConfig.UserInterface.Language;
		if (!Translations.ContainsKey(_currentLanguage)) _currentLanguage = UiLanguage.English;
		ApplyCulture(_currentLanguage);
	}

	public UiLanguage CurrentLanguage => _currentLanguage;

	public IEnumerable<UiLanguage> SupportedLanguages => Translations.Keys;

	public event EventHandler? LanguageChanged;

	public string this[string key] => Translate(key);

	public string Translate(string key) {
		if (Translations.TryGetValue(_currentLanguage, out var localized) && localized.TryGetValue(key, out var value)) return value;
		if (Translations.TryGetValue(UiLanguage.English, out var fallback) && fallback.TryGetValue(key, out var fallbackValue)) return fallbackValue;
		return key;
	}

	public string Format(string key, params object[] args) {
		string format = Translate(key);
		return args == null || args.Length == 0
			? format
			: string.Format(CultureInfo.CurrentCulture, format, args);
	}

	public void SetLanguage(UiLanguage language) {
		if (_currentLanguage == language) return;
		if (!Translations.ContainsKey(language)) language = UiLanguage.English;
		_currentLanguage = language;
		ApplyCulture(language);
		LanguageChanged?.Invoke(this, EventArgs.Empty);
	}

	public string GetLanguageDisplayName(UiLanguage language) {
		return LanguageDisplayNames.TryGetValue(language, out var name) ? name : language.ToString();
	}

	private void ApplyCulture(UiLanguage language) {
		string cultureName = CultureMap.TryGetValue(language, out var mappedCulture) ? mappedCulture : CultureMap[UiLanguage.English];
		CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
		CultureInfo.CurrentCulture = culture;
		CultureInfo.CurrentUICulture = culture;
	}
}
