using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace RatScanner;

public class Hotkey : INotifyPropertyChanged
{
	/// <summary>
	/// Keyboard key of the selected hotkey
	/// </summary>
	public HashSet<Key> KeyboardKeys;

	/// <summary>
	/// Mouse buttons of the selected hotkey
	/// </summary>
	public HashSet<MouseButton> MouseButtons;

	public bool RequiresKeyboard;

	public bool RequiresMouse;

	public Hotkey(Hotkey hotkey)
	{
		Set(hotkey);
	}

	public Hotkey(IEnumerable<Key>? keyboardKeys = null, IEnumerable<MouseButton>? mouseButtons = null)
	{
		Set(keyboardKeys, mouseButtons);
	}

	public void Set(Hotkey hotkey)
	{
		Set(hotkey.KeyboardKeys, hotkey.MouseButtons);
	}

	public void Set(IEnumerable<Key>? keyboardKeys, IEnumerable<MouseButton>? mouseButtons)
	{
		KeyboardKeys = keyboardKeys?.ToHashSet() ?? new HashSet<Key>();
		MouseButtons = mouseButtons?.ToHashSet() ?? new HashSet<MouseButton>();

		RequiresKeyboard = KeyboardKeys.Count > 0;
		RequiresMouse = MouseButtons.Count > 0;
	}

	public string HotkeyString
	{
		get
		{
			var keyboardString = string.Join('+', KeyboardKeys.ToList().OrderDescending());
			var mouseString = string.Join('+', MouseButtons.ToList().OrderDescending());

			var keyboardKeysEmpty = KeyboardKeys.Count == 0;
			var mouseButtonsEmpty = MouseButtons.Count == 0;
			if (!mouseButtonsEmpty && !keyboardKeysEmpty) return keyboardString + '+' + mouseString;
			return keyboardString + mouseString;
		}
	}

	public override string ToString()
	{
		return HotkeyString;
	}

	public event PropertyChangedEventHandler PropertyChanged;

	internal virtual void OnPropertyChanged(string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
