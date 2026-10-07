using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace RatScanner;

public enum UiLanguage {
	English = 0,
	Spanish = 1,
	French = 2,
	Polish = 3,
	Portuguese = 4,
	Russian = 5,
	Chinese = 6,
	German = 7,
}

public static class UiLanguageExtensions {
	public static string ToCultureName(this UiLanguage language) {
		return language switch {
			UiLanguage.English => "English",
			UiLanguage.Spanish => "Español",
			UiLanguage.French => "Français",
			UiLanguage.Polish => "Polski",
			UiLanguage.Portuguese => "Português",
			UiLanguage.Russian => "Русский",
			UiLanguage.Chinese => "中文",
			UiLanguage.German => "Deutsch",
			_ => "Unknown",
		};
	}

	public static string ToCultureCode(this UiLanguage language) {
		return language switch {
			UiLanguage.English => "en",
			UiLanguage.Spanish => "es",
			UiLanguage.French => "fr",
			UiLanguage.Polish => "pl",
			UiLanguage.Portuguese => "pt",
			UiLanguage.Russian => "ru",
			UiLanguage.Chinese => "zh",
			UiLanguage.German => "de",
			_ => "en",
		};
	}

	public static string GetTranslationFileName(this UiLanguage language) => $"{language.ToCultureCode()}.json";
}

public class LocalizationService {
	private static Dictionary<string, string>? Translations;

	public static void SetLanguage(UiLanguage language) {
		try {
			var filePath = Path.Combine(RatConfig.Paths.i18nDir, language.GetTranslationFileName());
			if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) {
				Logger.LogWarning($"Translation file not found: {filePath}");
			}
			var json = File.ReadAllText(filePath);
			Translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
		} catch (Exception ex) {
			// We do not want to crash the application if the translation file is missing or malformed
			Logger.LogWarning($"Failed to load translation file for language {language.ToCultureCode()}", ex);
		}
	}

	public LocalizationService() {
		SetLanguage(RatConfig.UserInterface.Language);
	}

	public string this[string key] => Translate(key);

	public static string Translate(string key) {
		if (Translations == null) {
			return key;
		} else if (Translations.TryGetValue(key, out var value)) {
			return value;
		} else {
			return key;
		}
	}

	public static string Format(string key, params object[] args) {
		var format = Translate(key);
		return args == null || args.Length == 0
			? format
			: string.Format(CultureInfo.CurrentCulture, format, args);
	}
}
