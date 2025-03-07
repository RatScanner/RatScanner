using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace RatScanner;

internal class SimpleConfig {
	internal string Path;
	internal string Section;
	internal string EnumerableSeparator = ";";

	[DllImport("kernel32")]
	private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

	[DllImport("kernel32")]
	private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

	internal SimpleConfig(string configPath, string section = "default") {
		Path = configPath;
		Section = section;
	}

	internal void WriteString(string key, string value) {
		WritePrivateProfileString(Section, key.ToLower(), value, Path);
	}
	internal void WriteSecureString(string key, string value) {
		if (string.IsNullOrEmpty(value)) {
			WriteString(key, value);
			return;
		}
		byte[] bytes = Encoding.ASCII.GetBytes(value);
		byte[] encryptedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
		string hexString = Convert.ToHexString(encryptedBytes);
		WritePrivateProfileString(Section, key.ToLower(), hexString, Path);
	}

	internal void WriteInt(string key, int value) {
		WriteString(key, value.ToString(CultureInfo.InvariantCulture));
	}

	internal void WriteFloat(string key, float value) {
		WriteString(key, value.ToString(CultureInfo.InvariantCulture));
	}

	internal void WriteBool(string key, bool value) {
		WriteString(key, value.ToString(CultureInfo.InvariantCulture));
	}

	internal void WriteEnumerableEnum<T>(string key, IEnumerable<T> value) where T : struct, IConvertible {
		if (value == null || !value.Any()) {
			WriteString(key, "null");
			return;
		}
		WriteString(key, string.Join(EnumerableSeparator, value));
	}

	internal void WriteHotkey(string key, Hotkey value) {
		WriteEnumerableEnum(key + "Keyboard", value.KeyboardKeys);
		WriteEnumerableEnum(key + "Mouse", value.MouseButtons);
	}

	private string ReadStringInternal(string key) {
		StringBuilder temp = new(1024);
		const string def = "RatScanner.Config.Default.Exception";
		GetPrivateProfileString(Section, key.ToLower(), def, temp, short.MaxValue, Path);
		string result = temp.ToString();
		return result == def ? throw new Exception(def) : result;
	}

	internal string ReadString(string key, string defaultValue) {
		try {
			return ReadStringInternal(key);
		} catch (Exception) {
			return defaultValue;
		}
	}

	internal string ReadSecureString(string key, string defaultValue) {
		try {
			string hexString = ReadStringInternal(key);
			if (string.IsNullOrEmpty(hexString)) return "";
			byte[] encryptedBytes = Convert.FromHexString(hexString);
			byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
			return Encoding.ASCII.GetString(decryptedBytes);
		} catch (Exception) {
			return defaultValue;
		}
	}

	internal int ReadInt(string key, int defaultValue) {
		try {
			return int.Parse(ReadStringInternal(key), CultureInfo.InvariantCulture);
		} catch (Exception) {
			return defaultValue;
		}
	}

	internal float ReadFloat(string key, float defaultValue) {
		try {
			return float.Parse(ReadStringInternal(key), CultureInfo.InvariantCulture);
		} catch (Exception) {
			return defaultValue;
		}
	}

	internal bool ReadBool(string key, bool defaultValue) {
		try {
			return bool.Parse(ReadStringInternal(key));
		} catch (Exception) {
			return defaultValue;
		}
	}

	internal IEnumerable<TEnum> ReadEnumerableEnum<TEnum>(string key, IEnumerable<TEnum> defaultValue) where TEnum : struct, Enum {
		try {
			string[]? readStrings = ReadStringInternal(key)?.Split(EnumerableSeparator);
			if (readStrings[0] == "null") return Enumerable.Empty<TEnum>();
			if (readStrings == null) return defaultValue;
			if (readStrings.Length == 1 && readStrings[0] == "") return defaultValue;
			return readStrings.Select(Enum.Parse<TEnum>);
		} catch (Exception) {
			return defaultValue;
		}
	}

	internal Hotkey ReadHotkey(string key, Hotkey? defaultValue) {
		defaultValue ??= new Hotkey();
		IEnumerable<System.Windows.Input.Key> keyboardKeys = ReadEnumerableEnum(key + "Keyboard", defaultValue.KeyboardKeys);
		IEnumerable<System.Windows.Input.MouseButton> mouseButtons = ReadEnumerableEnum(key + "Mouse", defaultValue.MouseButtons);
		return new Hotkey(keyboardKeys.ToList(), mouseButtons.ToList());
	}
}
