using System;
using System.Globalization;

namespace RatScanner;
public static class Extensions {
	public static string ToShortString(this int value) {
		string str = value.ToString();
		if (str.Length < 4) return str;

		string[] suffixes = new string[] { "", "K", "M", "B", "T", "Q" };

		string digits = str[..3];

		int dotPos = str.Length % 3;
		if (dotPos != 0) digits = digits[..dotPos];

		string suffix = suffixes[(int)Math.Floor((str.Length - 1) / 3f)];
		return $"{digits}{suffix}";
	}
	public static string ToShortString(this int? value) => ToShortString(value ?? 0);

	public static string AsRubs(this int value) {
		string text = $"{value:n0}";
		string numberGroupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
		text = text.Replace(numberGroupSeparator, RatConfig.ToolTip.DigitGroupingSymbol);
		return $"{text} ₽";
	}

	public static string AsRubs(this int? value) => AsRubs(value ?? 0);

	public static string AsRubs(this string value) => $"{value} ₽";

	public static TarkovDev.GraphQL.LanguageCode ToTarkovDevType(this RatStash.Language lang) => lang switch {
		RatStash.Language.Chinese => TarkovDev.GraphQL.LanguageCode.Zh,
		RatStash.Language.Czech => TarkovDev.GraphQL.LanguageCode.Cs,
		RatStash.Language.English => TarkovDev.GraphQL.LanguageCode.En,
		RatStash.Language.Spanish => TarkovDev.GraphQL.LanguageCode.Es,
		RatStash.Language.SpanishMexican => TarkovDev.GraphQL.LanguageCode.Es,
		RatStash.Language.French => TarkovDev.GraphQL.LanguageCode.Fr,
		RatStash.Language.German => TarkovDev.GraphQL.LanguageCode.De,
		RatStash.Language.Hungarian => TarkovDev.GraphQL.LanguageCode.Hu,
		RatStash.Language.Italian => TarkovDev.GraphQL.LanguageCode.It,
		RatStash.Language.Japanese => TarkovDev.GraphQL.LanguageCode.Ja,
		RatStash.Language.Korean => TarkovDev.GraphQL.LanguageCode.Ko,
		RatStash.Language.Polish => TarkovDev.GraphQL.LanguageCode.Pl,
		RatStash.Language.Portuguese => TarkovDev.GraphQL.LanguageCode.Pt,
		RatStash.Language.Russian => TarkovDev.GraphQL.LanguageCode.Ru,
		RatStash.Language.Slovak => TarkovDev.GraphQL.LanguageCode.Sk,
		RatStash.Language.Turkish => TarkovDev.GraphQL.LanguageCode.Tr,
		_ => TarkovDev.GraphQL.LanguageCode.En
	};
}
