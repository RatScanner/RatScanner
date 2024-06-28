using System;
using System.Globalization;

namespace RatScanner;
public class Price
{
	public enum CurrencyType
	{
		Rubel = '₽', 
		Dollar = '$',
		Euro = '€',
	}

	public readonly int Value;

	public readonly CurrencyType Currency;

	public Price(int value, CurrencyType currency = CurrencyType.Rubel)
	{
		Value = value;
		Currency = currency;
	}

	public override string ToString()
	{
		var text = $"{Value:n0}";
		var numberGroupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
		text = text.Replace(numberGroupSeparator, RatConfig.ToolTip.DigitGroupingSymbol);
		return $"{text} {(char)Currency}";
	}


	public string ToShortString()
	{
		if (Value-1000 < 0) return $"{(char)Currency} {Value}";
		return $"{(char)Currency} {Value.ToShortString()}";
	}
}
