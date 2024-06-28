﻿using RatStash;
using System;

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
}
