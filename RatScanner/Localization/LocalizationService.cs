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
		[UiLanguage.Russian] = new Dictionary<string, string> {
			["DashboardNav"] = "Панель",
			["SettingsNavGroup"] = "Настройки",
			["GeneralNav"] = "Общие",
			["ScanningNav"] = "Сканирование",
			["TrackingNav"] = "Отслеживание",
			["MinimalNav"] = "Мини-интерфейс",
			["OverlayNav"] = "Оверлей",
			["SaveButton"] = "Сохранить",
			["CancelButton"] = "Отмена",
			["GeneralSettingsTitle"] = "Общие настройки",
			["TooltipDuration"] = "Время подсказки: {0}",
			["ScreenResolution"] = "Разрешение экрана",
			["WidthLabel"] = "Ширина",
			["HeightLabel"] = "Высота",
			["ScaleLabel"] = "Масштаб",
			["UsePveData"] = "Использовать PvE данные",
			["AlwaysOnTop"] = "Всегда сверху",
			["LogDebugInfo"] = "Писать отладку",
			["InterfaceLanguage"] = "Язык интерфейса",
			["ScanningSettingsTitle"] = "Настройки сканирования",
			["NameScanLanguage"] = "Язык сканера имён",
			["EnableNameScan"] = "Включить сканер имён",
			["AutomaticScanning"] = "Автосканирование",
			["EnableIconScan"] = "Включить сканер иконок",
			["ScanRotatedIcons"] = "Сканировать повернутые иконки",
			["UseCachedIcons"] = "Использовать кэш иконок",
			["IconScanHotkey"] = "Горячая клавиша сканера иконок",
			["OverlaySettingsTitle"] = "Настройки оверлея",
			["EnableSearchOverlay"] = "Включить оверлей поиска",
			["BlurBehindSearch"] = "Размытие позади поиска",
			["OpenSearchOverlay"] = "Открыть оверлей поиска",
			["TrackingSettingsTitle"] = "Настройки отслеживания",
			["ShowNonFirNeeds"] = "Показывать требования не-FIR",
			["ShowKappaNeeds"] = "Показывать требования для Kappa",
			["BackendLabel"] = "Бэкенд",
			["ApiToken"] = "API токен",
			["ShowTeammates"] = "Показывать напарников",
			["InvalidToken"] = "Неверный токен",
			["TarkovTrackerTitle"] = "TarkovTracker",
			["MinimalUiSettingsTitle"] = "Настройки мини-интерфейса",
			["NameLabel"] = "Название",
			["AvgDailyPrice"] = "Сред. цена за день",
			["PricePerSlot"] = "Цена за слот",
			["TraderPrice"] = "Цена у торговца",
			["KappaNeeded"] = "Нужно для Kappa",
			["NeededQuestHideout"] = "Нужные квесты/укрытие",
			["NeededQuestHideoutTeam"] = "Нужные командные квесты/укрытие",
			["UpdatedTimestamp"] = "Обновлено",
			["OpacityLabel"] = "Прозрачность: {0}",
			["RecentAvgPrice"] = "Свежая сред. цена",
			["ValuePerSlot"] = "Ценность за слот",
			["BestTrader"] = "Лучший торговец",
			["QuestLabel"] = "Квест",
			["HideoutLabel"] = "Убежище",
			["ItemSearch"] = "Поиск предметов",
			["OpenWiki"] = "Открыть вики",
			["OpenTarkovDev"] = "Открыть Tarkov.dev",
			["FavoriteItem"] = "Избранный предмет",
			["AddItem"] = "Добавить предмет",
			["RemoveItem"] = "Удалить предмет",
			["NoneLabel"] = "Нет",
		},
		[UiLanguage.Chinese] = new Dictionary<string, string> {
			["DashboardNav"] = "仪表板",
			["SettingsNavGroup"] = "设置",
			["GeneralNav"] = "常规",
			["ScanningNav"] = "扫描",
			["TrackingNav"] = "追踪",
			["MinimalNav"] = "精简界面",
			["OverlayNav"] = "悬浮层",
			["SaveButton"] = "保存",
			["CancelButton"] = "取消",
			["GeneralSettingsTitle"] = "常规设置",
			["TooltipDuration"] = "提示持续时间：{0}",
			["ScreenResolution"] = "屏幕分辨率",
			["WidthLabel"] = "宽度",
			["HeightLabel"] = "高度",
			["ScaleLabel"] = "缩放",
			["UsePveData"] = "使用 PvE 数据",
			["AlwaysOnTop"] = "置顶显示",
			["LogDebugInfo"] = "记录调试信息",
			["InterfaceLanguage"] = "界面语言",
			["ScanningSettingsTitle"] = "扫描设置",
			["NameScanLanguage"] = "名称扫描语言",
			["EnableNameScan"] = "启用名称扫描",
			["AutomaticScanning"] = "自动扫描",
			["EnableIconScan"] = "启用图标扫描",
			["ScanRotatedIcons"] = "扫描旋转图标",
			["UseCachedIcons"] = "使用缓存图标",
			["IconScanHotkey"] = "图标扫描快捷键",
			["OverlaySettingsTitle"] = "悬浮层设置",
			["EnableSearchOverlay"] = "启用搜索悬浮层",
			["BlurBehindSearch"] = "搜索背景虚化",
			["OpenSearchOverlay"] = "打开搜索悬浮层",
			["TrackingSettingsTitle"] = "追踪设置",
			["ShowNonFirNeeds"] = "显示非FIR需求",
			["ShowKappaNeeds"] = "显示 Kappa 需求",
			["BackendLabel"] = "后端",
			["ApiToken"] = "API 令牌",
			["ShowTeammates"] = "显示队友",
			["InvalidToken"] = "令牌无效",
			["TarkovTrackerTitle"] = "TarkovTracker",
			["MinimalUiSettingsTitle"] = "精简界面设置",
			["NameLabel"] = "名称",
			["AvgDailyPrice"] = "日均价格",
			["PricePerSlot"] = "每格价格",
			["TraderPrice"] = "商人价格",
			["KappaNeeded"] = "Kappa 需求",
			["NeededQuestHideout"] = "所需任务/藏身处",
			["NeededQuestHideoutTeam"] = "团队任务/藏身处需求",
			["UpdatedTimestamp"] = "更新时间",
			["OpacityLabel"] = "不透明度：{0}",
			["RecentAvgPrice"] = "最新平均价格",
			["ValuePerSlot"] = "每格价值",
			["BestTrader"] = "最佳商人",
			["QuestLabel"] = "任务",
			["HideoutLabel"] = "藏身处",
			["ItemSearch"] = "搜索物品",
			["OpenWiki"] = "打开百科",
			["OpenTarkovDev"] = "打开 Tarkov.dev",
			["FavoriteItem"] = "收藏物品",
			["AddItem"] = "添加物品",
			["RemoveItem"] = "移除物品",
			["NoneLabel"] = "无",
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
