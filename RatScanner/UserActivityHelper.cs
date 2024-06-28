using RatEye;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace RatScanner;

internal static class UserActivityHelper
{
	[DllImport("user32.dll")]
	private static extern int GetAsyncKeyState(int vKey);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(ref Win32Point pt);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetModuleHandle(string name);

	[StructLayout(LayoutKind.Sequential)]
	private struct Win32Point
	{
		public int X;
		public int Y;
	}

	public static bool IsVKeyDown(int vKey)
	{
		return (GetAsyncKeyState(vKey) & 0x8000) != 0;
	}

	public static bool IsKeyDown(Key key)
	{
		var vKey = KeyInterop.VirtualKeyFromKey(key);
		return IsVKeyDown(vKey);
	}

	public static bool IsMouseButtonDown(MouseButton mouseButton)
	{
		var vKey = MouseButtonToVKey(mouseButton);
		return IsVKeyDown(vKey);
	}

	public static Vector2 GetMousePosition()
	{
		var w32Mouse = new Win32Point();
		GetCursorPos(ref w32Mouse);
		return new Vector2(w32Mouse.X, w32Mouse.Y);
	}

	public static int MouseButtonToVKey(MouseButton mouseButton)
	{
		return mouseButton switch
		{
			MouseButton.Left => 0x01,
			MouseButton.Right => 0x02,
			MouseButton.Middle => 0x04,
			MouseButton.XButton1 => 0x05,
			MouseButton.XButton2 => 0x06,
			_ => throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null),
		};
	}

	public static MouseButton VKeyToMouseButton(int vKey)
	{
		return vKey switch
		{
			0x01 => MouseButton.Left,
			0x02 => MouseButton.Right,
			0x04 => MouseButton.Middle,
			0x05 => MouseButton.XButton1,
			0x06 => MouseButton.XButton2,
			_ => throw new ArgumentOutOfRangeException(nameof(vKey), vKey, null),
		};
	}

	#region User activity hooks

	internal delegate void KeyUpEventHandler(object sender, KeyUpEventArgs e);

	internal delegate void KeyDownEventHandler(object sender, KeyDownEventArgs e);

	/// <summary>
	/// Occurs when the user releases any keyboard key
	/// </summary>
	internal static event KeyUpEventHandler OnKeyboardKeyUp;

	/// <summary>
	/// Occurs when the user releases any keyboard key
	/// </summary>
	internal static event KeyDownEventHandler OnKeyboardKeyDown;

	/// <summary>
	/// Occurs when the user releases any mouse button
	/// </summary>
	internal static event KeyUpEventHandler OnMouseButtonUp;

	/// <summary>
	/// Occurs when the user releases any mouse button
	/// </summary>
	internal static event KeyDownEventHandler OnMouseButtonDown;

	private const int WH_KEYBOARD_LL = 13;
	private const int WH_MOUSE_LL = 14;

	/// <summary>
	/// Stores the handle to the mouse hook procedure.
	/// </summary>
	private static int hMouseHook = 0;

	/// <summary>
	/// Stores the handle to the keyboard hook procedure.
	/// </summary>
	private static int hKeyboardHook = 0;

	private static HookProc MouseHookProcedure;
	private static HookProc KeyboardHookProcedure;

	private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

	[StructLayout(LayoutKind.Sequential)]
	private class MouseHookStruct
	{
		internal Win32Point pt;
		internal int hwnd;
		internal int wHitTestCode;
		internal int dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	private class MouseLLHookStruct
	{
		internal Win32Point pt;
		internal int mouseData;
		internal int flags;
		internal int time;
		internal int dwExtraInfo;
	}


	[StructLayout(LayoutKind.Sequential)]
	public class KBDLLHOOKSTRUCT
	{
		public uint vkCode;
		public uint scanCode;
		public KBDLLHOOKSTRUCTFlags flags;
		public uint time;
		public UIntPtr dwExtraInfo;
	}

	[Flags]
	public enum KBDLLHOOKSTRUCTFlags : uint
	{
		LLKHF_EXTENDED = 0x01,
		LLKHF_INJECTED = 0x10,
		LLKHF_ALTDOWN = 0x20,
		LLKHF_UP = 0x80,
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto,
		CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	private static extern int SetWindowsHookEx(
		int idHook,
		HookProc lpfn,
		IntPtr hMod,
		int dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto,
		CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	private static extern int UnhookWindowsHookEx(int idHook);

	[DllImport("user32.dll", CharSet = CharSet.Auto,
		CallingConvention = CallingConvention.StdCall)]
	private static extern int CallNextHookEx(
		int idHook,
		int nCode,
		int wParam,
		IntPtr lParam);

	internal static void Start(bool installMouseHook, bool installKeyboardHook)
	{
		// Install Mouse hook only if it is not installed and must be installed
		if (hMouseHook == 0 && installMouseHook)
		{
			// Create an instance of HookProc
			MouseHookProcedure = MouseHookProc;
			// Install hook
			hMouseHook = SetWindowsHookEx(
				WH_MOUSE_LL,
				MouseHookProcedure,
				GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
				0);
			// If SetWindowsHookEx fails
			if (hMouseHook == 0)
			{
				// Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set
				var errorCode = Marshal.GetLastWin32Error();
				// Do cleanup
				Stop(true, false, false);
				// Initializes and throws a new instance of the Win32Exception class with the specified error
				throw new Win32Exception(errorCode);
			}
		}

		// Install Keyboard hook only if it is not installed and must be installed
		if (hKeyboardHook == 0 && installKeyboardHook)
		{
			// Create an instance of HookProc
			KeyboardHookProcedure = KeyboardHookProc;
			// Install hook
			hKeyboardHook = SetWindowsHookEx(
				WH_KEYBOARD_LL,
				KeyboardHookProcedure,
				GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
				0);
			// If SetWindowsHookEx fails
			if (hKeyboardHook == 0)
			{
				// Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set
				var errorCode = Marshal.GetLastWin32Error();
				// Do cleanup
				Stop(false, true, false);
				// Initializes and throws a new instance of the Win32Exception class with the specified error
				throw new Win32Exception(errorCode);
			}
		}
	}

	/// <summary>
	/// Stops monitoring both or one of mouse and/or keyboard events and raising events.
	/// </summary>
	/// <param name="uninstallMouseHook"><b>true</b> if mouse hook must be uninstalled</param>
	/// <param name="uninstallKeyboardHook"><b>true</b> if keyboard hook must be uninstalled</param>
	/// <param name="throwExceptions"><b>true</b> if exceptions which occurred during uninstalling must be thrown</param>
	/// <exception cref="Win32Exception">Any windows problem.</exception>
	internal static void Stop(bool uninstallMouseHook, bool uninstallKeyboardHook, bool throwExceptions)
	{
		// If mouse hook set and must be uninstalled
		if (hMouseHook != 0 && uninstallMouseHook)
		{
			// Uninstall hook
			var retMouse = UnhookWindowsHookEx(hMouseHook);
			// Reset invalid handle
			hMouseHook = 0;
			// If failed and exception must be thrown
			if (retMouse == 0 && throwExceptions)
			{
				// Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set
				var errorCode = Marshal.GetLastWin32Error();
				// Initializes and throws a new instance of the Win32Exception class with the specified error
				throw new Win32Exception(errorCode);
			}
		}

		// If keyboard hook set and must be uninstalled
		if (hKeyboardHook != 0 && uninstallKeyboardHook)
		{
			// Uninstall hook
			var retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
			// Reset invalid handle
			hKeyboardHook = 0;
			// If failed and exception must be thrown
			if (retKeyboard == 0 && throwExceptions)
			{
				// Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set
				var errorCode = Marshal.GetLastWin32Error();
				// Initializes and throws a new instance of the Win32Exception class with the specified error
				throw new Win32Exception(errorCode);
			}
		}
	}

	private static int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
	{
		if (nCode < 0 || (OnKeyboardKeyUp == null && OnKeyboardKeyDown == null))
		{
			return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
		}

		// Indicates if any of underlying events set the Handled flag
		var handled = false;

		// Read structure KeyboardHookStruct at lParam
		var keyboardHookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

		// Raise OnKeyboardKeyUp or OnKeyboardKeyDown event
		if (OnKeyboardKeyUp != null && ((WM)wParam == WM.KEYUP || (WM)wParam == WM.SYSKEYUP))
		{
			var eventArgs = new KeyUpEventArgs((int)keyboardHookStruct.vkCode, Device.Keyboard);
			OnKeyboardKeyUp(null, eventArgs);
			handled |= eventArgs.Handled;
		}
		else if (OnKeyboardKeyDown != null && ((WM)wParam == WM.KEYDOWN || (WM)wParam == WM.SYSKEYDOWN))
		{
			var eventArgs = new KeyDownEventArgs((int)keyboardHookStruct.vkCode, Device.Keyboard);
			OnKeyboardKeyDown(null, eventArgs);
			handled |= eventArgs.Handled;
		}

		// If event is marked as handled, do not forward to other listeners
		return handled ? 1 : CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
	}

	private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
	{
		if (nCode < 0 || (OnMouseButtonUp == null && OnMouseButtonDown == null))
		{
			return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
		}

		// Indicates if any of underlying events set the Handled flag
		var handled = false;

		// Read structure MouseLLHookStruct at lParam
		var virtualKeycode = 0;

		var up = (WM)wParam switch
		{
			WM.LBUTTONUP => true,
			WM.RBUTTONUP => true,
			WM.MBUTTONUP => true,
			WM.XBUTTONUP => true,
			_ => false,
		};

		switch ((WM)wParam)
		{
			case WM.LBUTTONUP:
			case WM.LBUTTONDOWN:
				virtualKeycode = 0x01;
				break;
			case WM.RBUTTONUP:
			case WM.RBUTTONDOWN:
				virtualKeycode = 0x02;
				break;
			case WM.MBUTTONUP:
			case WM.MBUTTONDOWN:
				virtualKeycode = 0x04;
				break;
			case WM.XBUTTONUP:
			case WM.XBUTTONDOWN:
				var mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
				if (mouseHookStruct.mouseData == 0x10000) virtualKeycode = 0x05;
				else if (mouseHookStruct.mouseData == 0x20000) virtualKeycode = 0x06;
				break;
			default:
				return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
		}

		if (up && OnMouseButtonUp != null)
		{
			var eventArgs = new KeyUpEventArgs(virtualKeycode, Device.Mouse);
			OnMouseButtonUp(null, eventArgs);
			handled |= eventArgs.Handled;
		}
		else if (!up && OnMouseButtonDown != null)
		{
			var eventArgs = new KeyDownEventArgs(virtualKeycode, Device.Mouse);
			OnMouseButtonDown(null, eventArgs);
			handled |= eventArgs.Handled;
		}

		// If event is marked as handled, do not forward to other listeners
		return handled ? 1 : CallNextHookEx(hMouseHook, nCode, wParam, lParam);
	}

	#endregion
}

#region WM
public enum WM : uint
{
	NULL = 0x0000,
	CREATE = 0x0001,
	DESTROY = 0x0002,
	MOVE = 0x0003,
	SIZE = 0x0005,
	ACTIVATE = 0x0006,
	SETFOCUS = 0x0007,
	KILLFOCUS = 0x0008,
	ENABLE = 0x000A,
	SETREDRAW = 0x000B,
	SETTEXT = 0x000C,
	GETTEXT = 0x000D,
	GETTEXTLENGTH = 0x000E,
	PAINT = 0x000F,
	CLOSE = 0x0010,
	QUERYENDSESSION = 0x0011,
	QUERYOPEN = 0x0013,
	ENDSESSION = 0x0016,
	QUIT = 0x0012,
	ERASEBKGND = 0x0014,
	SYSCOLORCHANGE = 0x0015,
	SHOWWINDOW = 0x0018,
	WININICHANGE = 0x001A,
	SETTINGCHANGE = WININICHANGE,
	DEVMODECHANGE = 0x001B,
	ACTIVATEAPP = 0x001C,
	FONTCHANGE = 0x001D,
	TIMECHANGE = 0x001E,
	CANCELMODE = 0x001F,
	SETCURSOR = 0x0020,
	MOUSEACTIVATE = 0x0021,
	CHILDACTIVATE = 0x0022,
	QUEUESYNC = 0x0023,
	GETMINMAXINFO = 0x0024,
	PAINTICON = 0x0026,
	ICONERASEBKGND = 0x0027,
	NEXTDLGCTL = 0x0028,
	SPOOLERSTATUS = 0x002A,
	DRAWITEM = 0x002B,
	MEASUREITEM = 0x002C,
	DELETEITEM = 0x002D,
	VKEYTOITEM = 0x002E,
	CHARTOITEM = 0x002F,
	SETFONT = 0x0030,
	GETFONT = 0x0031,
	SETHOTKEY = 0x0032,
	GETHOTKEY = 0x0033,
	QUERYDRAGICON = 0x0037,
	COMPAREITEM = 0x0039,
	GETOBJECT = 0x003D,
	COMPACTING = 0x0041,
	COMMNOTIFY = 0x0044,
	WINDOWPOSCHANGING = 0x0046,
	WINDOWPOSCHANGED = 0x0047,
	POWER = 0x0048,
	COPYDATA = 0x004A,
	CANCELJOURNAL = 0x004B,
	NOTIFY = 0x004E,
	INPUTLANGCHANGEREQUEST = 0x0050,
	INPUTLANGCHANGE = 0x0051,
	TCARD = 0x0052,
	HELP = 0x0053,
	USERCHANGED = 0x0054,
	NOTIFYFORMAT = 0x0055,
	CONTEXTMENU = 0x007B,
	STYLECHANGING = 0x007C,
	STYLECHANGED = 0x007D,
	DISPLAYCHANGE = 0x007E,
	GETICON = 0x007F,
	SETICON = 0x0080,
	NCCREATE = 0x0081,
	NCDESTROY = 0x0082,
	NCCALCSIZE = 0x0083,
	NCHITTEST = 0x0084,
	NCPAINT = 0x0085,
	NCACTIVATE = 0x0086,
	GETDLGCODE = 0x0087,
	SYNCPAINT = 0x0088,
	NCMOUSEMOVE = 0x00A0,
	NCLBUTTONDOWN = 0x00A1,
	NCLBUTTONUP = 0x00A2,
	NCLBUTTONDBLCLK = 0x00A3,
	NCRBUTTONDOWN = 0x00A4,
	NCRBUTTONUP = 0x00A5,
	NCRBUTTONDBLCLK = 0x00A6,
	NCMBUTTONDOWN = 0x00A7,
	NCMBUTTONUP = 0x00A8,
	NCMBUTTONDBLCLK = 0x00A9,
	NCXBUTTONDOWN = 0x00AB,
	NCXBUTTONUP = 0x00AC,
	NCXBUTTONDBLCLK = 0x00AD,
	INPUT_DEVICE_CHANGE = 0x00FE,
	INPUT = 0x00FF,
	KEYFIRST = 0x0100,
	KEYDOWN = 0x0100,
	KEYUP = 0x0101,
	CHAR = 0x0102,
	DEADCHAR = 0x0103,
	SYSKEYDOWN = 0x0104,
	SYSKEYUP = 0x0105,
	SYSCHAR = 0x0106,
	SYSDEADCHAR = 0x0107,
	UNICHAR = 0x0109,
	KEYLAST = 0x0108,
	IME_STARTCOMPOSITION = 0x010D,
	IME_ENDCOMPOSITION = 0x010E,
	IME_COMPOSITION = 0x010F,
	IME_KEYLAST = 0x010F,
	INITDIALOG = 0x0110,
	COMMAND = 0x0111,
	SYSCOMMAND = 0x0112,
	TIMER = 0x0113,
	HSCROLL = 0x0114,
	VSCROLL = 0x0115,
	INITMENU = 0x0116,
	INITMENUPOPUP = 0x0117,
	MENUSELECT = 0x011F,
	MENUCHAR = 0x0120,
	ENTERIDLE = 0x0121,
	MENURBUTTONUP = 0x0122,
	MENUDRAG = 0x0123,
	MENUGETOBJECT = 0x0124,
	UNINITMENUPOPUP = 0x0125,
	MENUCOMMAND = 0x0126,
	CHANGEUISTATE = 0x0127,
	UPDATEUISTATE = 0x0128,
	QUERYUISTATE = 0x0129,
	CTLCOLORMSGBOX = 0x0132,
	CTLCOLOREDIT = 0x0133,
	CTLCOLORLISTBOX = 0x0134,
	CTLCOLORBTN = 0x0135,
	CTLCOLORDLG = 0x0136,
	CTLCOLORSCROLLBAR = 0x0137,
	CTLCOLORSTATIC = 0x0138,
	MOUSEFIRST = 0x0200,
	MOUSEMOVE = 0x0200,
	LBUTTONDOWN = 0x0201,
	LBUTTONUP = 0x0202,
	LBUTTONDBLCLK = 0x0203,
	RBUTTONDOWN = 0x0204,
	RBUTTONUP = 0x0205,
	RBUTTONDBLCLK = 0x0206,
	MBUTTONDOWN = 0x0207,
	MBUTTONUP = 0x0208,
	MBUTTONDBLCLK = 0x0209,
	MOUSEWHEEL = 0x020A,
	XBUTTONDOWN = 0x020B,
	XBUTTONUP = 0x020C,
	XBUTTONDBLCLK = 0x020D,
	MOUSEHWHEEL = 0x020E,
	MOUSELAST = 0x020E,
	PARENTNOTIFY = 0x0210,
	ENTERMENULOOP = 0x0211,
	EXITMENULOOP = 0x0212,
	NEXTMENU = 0x0213,
	SIZING = 0x0214,
	CAPTURECHANGED = 0x0215,
	MOVING = 0x0216,
	POWERBROADCAST = 0x0218,
	DEVICECHANGE = 0x0219,
	MDICREATE = 0x0220,
	MDIDESTROY = 0x0221,
	MDIACTIVATE = 0x0222,
	MDIRESTORE = 0x0223,
	MDINEXT = 0x0224,
	MDIMAXIMIZE = 0x0225,
	MDITILE = 0x0226,
	MDICASCADE = 0x0227,
	MDIICONARRANGE = 0x0228,
	MDIGETACTIVE = 0x0229,
	MDISETMENU = 0x0230,
	ENTERSIZEMOVE = 0x0231,
	EXITSIZEMOVE = 0x0232,
	DROPFILES = 0x0233,
	MDIREFRESHMENU = 0x0234,
	IME_SETCONTEXT = 0x0281,
	IME_NOTIFY = 0x0282,
	IME_CONTROL = 0x0283,
	IME_COMPOSITIONFULL = 0x0284,
	IME_SELECT = 0x0285,
	IME_CHAR = 0x0286,
	IME_REQUEST = 0x0288,
	IME_KEYDOWN = 0x0290,
	IME_KEYUP = 0x0291,
	MOUSEHOVER = 0x02A1,
	MOUSELEAVE = 0x02A3,
	NCMOUSEHOVER = 0x02A0,
	NCMOUSELEAVE = 0x02A2,
	WTSSESSION_CHANGE = 0x02B1,
	TABLET_FIRST = 0x02c0,
	TABLET_LAST = 0x02df,
	CUT = 0x0300,
	COPY = 0x0301,
	PASTE = 0x0302,
	CLEAR = 0x0303,
	UNDO = 0x0304,
	RENDERFORMAT = 0x0305,
	RENDERALLFORMATS = 0x0306,
	DESTROYCLIPBOARD = 0x0307,
	DRAWCLIPBOARD = 0x0308,
	PAINTCLIPBOARD = 0x0309,
	VSCROLLCLIPBOARD = 0x030A,
	SIZECLIPBOARD = 0x030B,
	ASKCBFORMATNAME = 0x030C,
	CHANGECBCHAIN = 0x030D,
	HSCROLLCLIPBOARD = 0x030E,
	QUERYNEWPALETTE = 0x030F,
	PALETTEISCHANGING = 0x0310,
	PALETTECHANGED = 0x0311,
	HOTKEY = 0x0312,
	PRINT = 0x0317,
	PRINTCLIENT = 0x0318,
	APPCOMMAND = 0x0319,
	THEMECHANGED = 0x031A,
	CLIPBOARDUPDATE = 0x031D,
	DWMCOMPOSITIONCHANGED = 0x031E,
	DWMNCRENDERINGCHANGED = 0x031F,
	DWMCOLORIZATIONCOLORCHANGED = 0x0320,
	DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
	GETTITLEBARINFOEX = 0x033F,
	HANDHELDFIRST = 0x0358,
	HANDHELDLAST = 0x035F,
	AFXFIRST = 0x0360,
	AFXLAST = 0x037F,
	PENWINFIRST = 0x0380,
	PENWINLAST = 0x038F,
	APP = 0x8000,
	USER = 0x0400,

	CPL_LAUNCH = USER + 0x1000,
	CPL_LAUNCHED = USER + 0x1001,
	SYSTIMER = 0x118,

	HSHELL_ACCESSIBILITYSTATE = 11,
	HSHELL_ACTIVATESHELLWINDOW = 3,
	HSHELL_APPCOMMAND = 12,
	HSHELL_GETMINRECT = 5,
	HSHELL_LANGUAGE = 8,
	HSHELL_REDRAW = 6,
	HSHELL_TASKMAN = 7,
	HSHELL_WINDOWCREATED = 1,
	HSHELL_WINDOWDESTROYED = 2,
	HSHELL_WINDOWACTIVATED = 4,
	HSHELL_WINDOWREPLACED = 13
}
#endregion

internal class KeyUpEventArgs : EventArgs
{
	internal bool Handled = false;

	internal int VKCode;

	internal Key Key
	{
		get
		{
			var message = "Trying to access Key of non keyboard event. Check device property first.";
			if (Device != Device.Keyboard) throw new Exception(message);
			return KeyInterop.KeyFromVirtualKey(VKCode);
		}
	}

	internal MouseButton MouseButton
	{
		get
		{
			var message = "Trying to access MouseButton of non mouse event. Check device property first.";
			if (Device != Device.Mouse) throw new Exception(message);
			return UserActivityHelper.VKeyToMouseButton(VKCode);
		}
	}

	internal readonly Device Device;

	internal KeyUpEventArgs(int vkCode, Device device)
	{
		VKCode = vkCode;
		Device = device;
	}
}

internal class KeyDownEventArgs : EventArgs
{
	internal bool Handled = false;

	internal int VKCode;

	internal Key Key => KeyInterop.KeyFromVirtualKey(VKCode);

	internal MouseButton MouseButton => UserActivityHelper.VKeyToMouseButton(VKCode);

	internal Device Device;

	internal KeyDownEventArgs(int vkCode, Device device)
	{
		VKCode = vkCode;
		Device = device;
	}
}

internal enum Device
{
	Keyboard,
	Mouse,
}
