using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RatScanner;

public static class Extensions {
	public static string ToShortString(this int value) {
		var str = value.ToString();
		if (str.Length < 4) return str;

		string[] suffixes = ["", "K", "M", "B", "T", "Q"];

		var digits = str[..3];

		var dotPos = str.Length % 3;
		if (dotPos != 0) digits = digits[..dotPos];

		var suffix = suffixes[(int)Math.Floor((str.Length - 1) / 3f)];
		return $"{digits}{suffix}";
	}
	public static string ToShortString(this int? value) => ToShortString(value ?? 0);

	public static string AsRubs(this int value) {
		var text = $"{value:n0}";
		var numberGroupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
		text = text.Replace(numberGroupSeparator, RatConfig.ToolTip.DigitGroupingSymbol);
		return $"{text} ₽";
	}

	public static string AsRubs(this int? value) => AsRubs(value ?? 0);

	public static string AsRubs(this string value) => $"{value} ₽";

	public static string ToApiLanguageCode(this RatStash.Language lang) => lang switch {
		RatStash.Language.Chinese => "zh",
		RatStash.Language.Czech => "cs",
		RatStash.Language.English => "en",
		RatStash.Language.Spanish => "es",
		RatStash.Language.SpanishMexican => "es",
		RatStash.Language.French => "fr",
		RatStash.Language.German => "de",
		RatStash.Language.Hungarian => "hu",
		RatStash.Language.Italian => "it",
		RatStash.Language.Japanese => "ja",
		RatStash.Language.Korean => "ko",
		RatStash.Language.Polish => "pl",
		RatStash.Language.Portuguese => "pt",
		RatStash.Language.Russian => "ru",
		RatStash.Language.Slovak => "sk",
		RatStash.Language.Turkish => "tr",
		_ => "en"
	};

	public static T Random<T>(this IEnumerable<T> source) {
		ArgumentNullException.ThrowIfNull(source);
		var list = source as IList<T> ?? source.ToList();
		if (list.Count == 0) throw new InvalidOperationException("Sequence contains no elements.");
		return list[System.Random.Shared.Next(list.Count)];
	}
}
