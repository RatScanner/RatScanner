using RatStash;
using System;
using System.Globalization;

namespace RatScanner;
public static class Extensions
{
	public static string ToShortString(this int value)
	{
		var str = value.ToString();
		if (str.Length < 4) return str;

		var suffixes = new string[] { "", "K", "M", "B", "T", "Q" };

		var digits = str[..3];

		var dotPos = str.Length % 3;
		if (dotPos != 0) digits = digits[..dotPos];

		var suffix = suffixes[(int)Math.Floor((str.Length - 1) / 3f)];
		return $"{digits}{suffix}";
	}
	public static string ToShortString(this int? value) => ToShortString(value ?? 0);

	public static string AsRubs(this int value)
	{
		var text = $"{value:n0}";
		var numberGroupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
		text = text.Replace(numberGroupSeparator, RatConfig.ToolTip.DigitGroupingSymbol);
		return $"{text} ₽";
	}

	public static string AsRubs(this int? value) => AsRubs(value ?? 0);

	public static string AsRubs(this string value) => $"{value} ₽";
}
