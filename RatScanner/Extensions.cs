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
}
