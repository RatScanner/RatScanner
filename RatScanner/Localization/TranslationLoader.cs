using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RatScanner.Localization;

internal static class TranslationLoader {
	public static bool TryLoad(string filePath, out IReadOnlyDictionary<string, string> translations) {
		translations = new Dictionary<string, string>();
		if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;

		using FileStream fileStream = File.OpenRead(filePath);
		var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(fileStream);
		if (payload is null) return false;

		translations = payload;
		return true;
	}
}
