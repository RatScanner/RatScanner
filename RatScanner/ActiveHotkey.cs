using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using RatScanner.Controls;
using static RatScanner.UserActivityHelper;

namespace RatScanner
{
	internal class ActiveHotkey : Hotkey, IDisposable
	{
		private event KeyUpEventHandler HotkeyPressedEventHandler;

		/// <summary>
		/// <see langword="true"/> if the hotkey should not be forwarded down
		/// </summary>
		internal bool SuppressHotkey = false;

		internal bool Enabled = false;

		/// <summary>
		/// Create a new active hotkey which will notify the event handler, when the hotkey is pressed
		/// </summary>
		/// <param name="hotkey">The hotkey which will be listened for</param>
		/// <param name="hotkeyPressedEventHandler">The event handler which will be notified</param>
		/// <param name="suppressHotkey"><see langword="true"/> if the hotkey should not be forwarded down the chain</param>
		internal ActiveHotkey(Hotkey hotkey, KeyUpEventHandler hotkeyPressedEventHandler, bool suppressHotkey = false) : base(hotkey.KeyboardKeys, hotkey.MouseButtons)
		{
			HotkeyPressedEventHandler += hotkeyPressedEventHandler;
			SuppressHotkey = suppressHotkey;
			RegisterEventListeners();
		}

		/// <summary>
		/// Create a new active hotkey which will notify the event handler, when the hotkey is pressed
		/// </summary>
		/// <param name="hotkey">The hotkey which will be listened for</param>
		/// <param name="hotkeyPressedEventHandler">The event handler which will be notified</param>
		/// <param name="enabled"><see langword="false"/> to disable the active hotkey by reference</param>
		/// <param name="suppressHotkey"><see langword="true"/> if the hotkey should not be forwarded down the chain</param>
		internal ActiveHotkey(Hotkey hotkey, KeyUpEventHandler hotkeyPressedEventHandler, ref bool enabled, bool suppressHotkey = false) : base(hotkey.KeyboardKeys, hotkey.MouseButtons)
		{
			HotkeyPressedEventHandler += hotkeyPressedEventHandler;
			Enabled = enabled;
			SuppressHotkey = suppressHotkey;
			RegisterEventListeners();
		}

		/// <summary>
		/// Create a new active hotkey which will notify the event handler, when the hotkey is pressed
		/// </summary>
		/// <param name="keyboardKeys">The keyboard keys of the hotkey which will be listened for</param>
		/// <param name="mouseButtons">The mouse buttons of the hotkey which will be listened for</param>
		/// <param name="hotkeyPressedEventHandler">The event handler which will be notified</param>
		/// <param name="suppressHotkey"><see langword="true"/> if the hotkey should not be forwarded down the chain</param>
		internal ActiveHotkey(List<Key> keyboardKeys, List<MouseButton> mouseButtons, KeyUpEventHandler hotkeyPressedEventHandler, bool suppressHotkey = false) : base(keyboardKeys, mouseButtons)
		{
			HotkeyPressedEventHandler += hotkeyPressedEventHandler;
			SuppressHotkey = suppressHotkey;
			RegisterEventListeners();
		}

		/// <summary>
		/// Create a new active hotkey which will notify the event handler, when the hotkey is pressed
		/// </summary>
		/// <param name="keyboardKeys">The keyboard keys of the hotkey which will be listened for</param>
		/// <param name="mouseButtons">The mouse buttons of the hotkey which will be listened for</param>
		/// <param name="hotkeyPressedEventHandler">The event handler which will be notified</param>
		/// <param name="enabled"><see langword="false"/> to disable the active hotkey by reference</param>
		/// <param name="suppressHotkey"><see langword="true"/> if the hotkey should not be forwarded down the chain</param>
		internal ActiveHotkey(List<Key> keyboardKeys, List<MouseButton> mouseButtons, KeyUpEventHandler hotkeyPressedEventHandler, ref bool enabled, bool suppressHotkey = false) : base(keyboardKeys, mouseButtons)
		{
			HotkeyPressedEventHandler += hotkeyPressedEventHandler;
			SuppressHotkey = suppressHotkey;
			Enabled = enabled;
			RegisterEventListeners();
		}

		private void RegisterEventListeners()
		{
			if (RequiresKeyboard) OnKeyboardKeyUp += OnKeyUp;
			if (RequiresMouse) OnMouseButtonUp += OnKeyUp;
		}

		private void UnregisterEventListeners()
		{
			if (RequiresKeyboard) OnKeyboardKeyUp -= OnKeyUp;
			if (RequiresMouse) OnMouseButtonUp -= OnKeyUp;
		}

		private void OnKeyUp(object sender, KeyUpEventArgs e)
		{
			if (!Enabled) return;
			if (IsPressed(e) && HotkeyPressedEventHandler != null)
			{
				Logger.LogDebug("Pressed: " + ToString());
				e.Handled |= SuppressHotkey;
				Task.Run(() => HotkeyPressedEventHandler(sender, e));
			}
		}

		internal bool IsPressed(KeyUpEventArgs e)
		{
			if (e == null) throw new ArgumentNullException(nameof(e), "KeyUpEventArgs can not be empty!");

			var keyDownInHotkey = false;

			if (RequiresKeyboard)
			{
				foreach (var keyboardKey in KeyboardKeys)
				{
					var vKey = KeyInterop.VirtualKeyFromKey(keyboardKey);
					if (!IsKeyDown(vKey)) return false;
					keyDownInHotkey |= e.Key == KeyInterop.VirtualKeyFromKey(keyboardKey);
				}
			}

			if (RequiresMouse)
			{
				foreach (var mouseButton in MouseButtons)
				{
					var vKey = MouseButtonToVKey(mouseButton);
					if (!IsKeyDown(vKey)) return false;
					keyDownInHotkey |= vKey == e.Key;
				}
			}

			return keyDownInHotkey;
		}

		public void Dispose()
		{
			UnregisterEventListeners();
			HotkeyPressedEventHandler = null;
		}
	}
}
