using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RatScanner
{
	internal class SimpleConfig
	{
		internal string Path;
		internal string Section;
		internal string EnumerableSeparator = ";";

		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

		internal SimpleConfig(string configPath, string section = "default")
		{
			Path = configPath;
			Section = section;
		}

		internal void WriteString(string key, string value)
		{
			WritePrivateProfileString(Section, key.ToLower(), value, Path);
		}

		internal void WriteInt(string key, int value)
		{
			WriteString(key, value.ToString());
		}

		internal void WriteFloat(string key, float value)
		{
			WriteString(key, value.ToString(CultureInfo.InvariantCulture));
		}

		internal void WriteBool(string key, bool value)
		{
			WriteString(key, value.ToString());
		}

		internal void WriteEnumerableEnum<T>(string key, IEnumerable<T> value) where T : struct, IConvertible
		{
			WriteString(key, string.Join(EnumerableSeparator, value));
		}

		internal string ReadString(string key, string defaultValue = "")
		{
			try
			{
				var temp = new StringBuilder(255);
				GetPrivateProfileString(Section, key.ToLower(), defaultValue, temp, short.MaxValue, Path);
				return temp.ToString();
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}

		internal int ReadInt(string key, int defaultValue = 0)
		{
			try
			{
				return int.Parse(ReadString(key));
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}

		internal float ReadFloat(string key, float defaultValue = 0)
		{
			try
			{
				return float.Parse(ReadString(key));
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}

		internal bool ReadBool(string key, bool defaultValue = false)
		{
			try
			{
				return bool.Parse(ReadString(key));
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}

		internal IEnumerable<TEnum> ReadEnumerableEnum<TEnum>(string key, IEnumerable<TEnum> defaultValue = null) where TEnum : struct, Enum
		{
			try
			{
				var readStrings = ReadString(key).Split(EnumerableSeparator);
				if (readStrings.Length == 1 && readStrings[0] == "") return new TEnum[0];
				return readStrings.Select(Enum.Parse<TEnum>);
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}
	}
}
