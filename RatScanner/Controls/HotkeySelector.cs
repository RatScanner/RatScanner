using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RatScanner.Controls
{
	public class Hotkey
	{
		/// <summary>
		/// Keyboard key of the selected hotkey
		/// </summary>
		public readonly List<Key> KeyboardKeys;

		/// <summary>
		/// Mouse buttons of the selected hotkey
		/// </summary>
		public readonly List<MouseButton> MouseButtons;

		public readonly bool RequiresKeyboard;

		public readonly bool RequiresMouse;

		public Hotkey(IEnumerable<Key> keyboardKeys = null, IEnumerable<MouseButton> mouseButtons = null)
		{
			KeyboardKeys = keyboardKeys?.ToList() ?? new List<Key>();
			MouseButtons = mouseButtons?.ToList() ?? new List<MouseButton>();

			RequiresKeyboard = KeyboardKeys.Count > 0;
			RequiresMouse = MouseButtons.Count > 0;
		}

		public override string ToString()
		{
			var keyboardString = string.Join('+', KeyboardKeys);
			var mouseString = string.Join('+', MouseButtons);

			var keyboardKeysEmpty = KeyboardKeys.Count == 0;
			var mouseButtonsEmpty = MouseButtons.Count == 0;
			if (!mouseButtonsEmpty && !keyboardKeysEmpty)
			{
				return keyboardString + '+' + mouseString;
			}
			return keyboardString + mouseString;
		}
	}

	public class HotkeySelector : Button
	{
		/// <summary>
		/// Identifies the <see cref="DoCaptureMouse"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DoCaptureMouseProperty = DependencyProperty.Register(
			nameof(DoCaptureMouse),
			typeof(bool),
			typeof(HotkeySelector),
			new PropertyMetadata(false));

		/// <summary>
		/// Identifies the <see cref="ListeningBackground"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ListeningBackgroundProperty = DependencyProperty.Register(
			nameof(ListeningBackground),
			typeof(Brush),
			typeof(HotkeySelector));

		/// <summary>
		/// Identifies the <see cref="Hotkey"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty HotkeyProperty = DependencyProperty.Register(
			nameof(Hotkey),
			typeof(Hotkey),
			typeof(HotkeySelector),
			new PropertyMetadata(new Hotkey()));

		/// <summary>
		/// Makes the control also listen for mouse buttons when waiting for a hotkey sequence
		/// </summary>
		public bool DoCaptureMouse
		{
			get => (bool)GetValue(DoCaptureMouseProperty);
			set => SetValue(DoCaptureMouseProperty, value);
		}

		/// <summary>
		/// Background brush of the control when actively listening for a hotkey sequence
		/// </summary>
		public Brush ListeningBackground
		{
			get => (Brush)GetValue(ListeningBackgroundProperty);
			set => SetValue(ListeningBackgroundProperty, value);
		}

		/// <summary>
		/// Hotkey of the control
		/// </summary>
		public Hotkey Hotkey
		{
			get => (Hotkey)GetValue(HotkeyProperty);
			set
			{
				SetValue(HotkeyProperty, value);
				UpdateControl();
			}
		}

		private bool _listening = false;
		private Brush _previousBackground;

		static HotkeySelector()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(HotkeySelector), new FrameworkPropertyMetadata(typeof(HotkeySelector)));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			UpdateControl();
		}

		protected override void OnClick()
		{
			if (_listening) return;
			StartListening();
			base.OnClick();
		}

		/// <summary>
		/// Add keyboard keys to list while listening
		/// </summary>
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (!_listening) return;
			if (Hotkey.KeyboardKeys.Contains(e.Key)) return;

			Hotkey.KeyboardKeys.Add(e.Key);
			UpdateControl();

			e.Handled = true;
		}

		/// <summary>
		/// Stop listening for keys as soon as a single key is released
		/// </summary>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			StopListening();
		}

		/// <summary>
		/// Add mouse buttons to list while listening
		/// </summary>
		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			if (!DoCaptureMouse)
			{
				base.OnPreviewMouseDown(e);
				return;
			}

			if (!_listening) return;
			if (Hotkey.MouseButtons.Contains(e.ChangedButton)) return;

			Hotkey.MouseButtons.Add(e.ChangedButton);
			UpdateControl();

			e.Handled = true;
		}

		/// <summary>
		/// Stop listening for keys as soon as a single key is released
		/// </summary>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (!DoCaptureMouse)
			{
				base.OnMouseUp(e);
				return;
			}

			StopListening();
		}

		/// <summary>
		/// Stop listening for keys when the control loses focus
		/// </summary>
		protected override void OnLostFocus(RoutedEventArgs e)
		{
			StopListening();
			base.OnLostFocus(e);
		}

		private void StartListening()
		{
			if (_listening) return;

			Hotkey.KeyboardKeys.Clear();
			Hotkey.MouseButtons.Clear();

			// Capture mouse if the control is supposed to
			if (DoCaptureMouse) Mouse.Capture(this, CaptureMode.Element);

			// Update background
			_previousBackground = Background;
			SetValue(BackgroundProperty, ListeningBackground);

			_listening = true;
		}

		private void StopListening()
		{
			if (!_listening) return;

			_listening = false;

			// Release mouse capture
			ReleaseMouseCapture();

			// Reset background
			SetValue(BackgroundProperty, _previousBackground);
		}

		private void UpdateControl()
		{
			Sort();
			Content = ToString();
		}

		private void Sort()
		{
			Hotkey.KeyboardKeys.Sort();
			Hotkey.MouseButtons.Sort();
		}

		public override string ToString()
		{
			return Hotkey.ToString();
		}
	}
}
