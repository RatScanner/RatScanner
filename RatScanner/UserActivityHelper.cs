using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace RatScanner
{
	internal static class UserActivityHelper
	{
		[DllImport("user32.dll")]
		private static extern int GetAsyncKeyState(int vKey);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(ref Win32Point pt);

		[StructLayout(LayoutKind.Sequential)]
		private struct Win32Point
		{
			public int X;
			public int Y;
		}

		public static bool IsKeyDown(int vKey)
		{
			return (GetAsyncKeyState(vKey) & 0x8000) != 0;
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
				_ => throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null)
			};
		}

		#region User activity hooks

		internal delegate void KeyUpEventHandler(object sender, KeyUpEventArgs e);

		/// <summary>
		/// Occurs when the user releases a keyboard key
		/// </summary>
		internal static event KeyUpEventHandler OnKeyboardKeyUp;

		/// <summary>
		/// Occurs when the user releases any mouse button
		/// </summary>
		internal static event KeyUpEventHandler OnMouseButtonUp;

		/// <summary>
		/// Windows NT/2000/XP: Installs a hook procedure that monitors low-level mouse input events.
		/// </summary>
		private const int WH_MOUSE_LL = 14;

		/// <summary>
		/// Windows NT/2000/XP: Installs a hook procedure that monitors low-level keyboard  input events.
		/// </summary>
		private const int WH_KEYBOARD_LL = 13;

		/// <summary>
		/// The WM_KEYUP message is posted to the window with the keyboard focus when a nonsystem 
		/// key is released. A nonsystem key is a key that is pressed when the ALT key is not pressed, 
		/// or a keyboard key that is pressed when a window has the keyboard focus.
		/// </summary>
		private const int WM_KEYUP = 0x101;

		/// <summary>
		/// The WM_SYSKEYUP message is posted to the window with the keyboard focus when the user 
		/// releases a key that was pressed while the ALT key was held down. It also occurs when no 
		/// window currently has the keyboard focus; in this case, the WM_SYSKEYUP message is sent 
		/// to the active window. The window that receives the message can distinguish between 
		/// these two contexts by checking the context code in the lParam parameter. 
		/// </summary>
		private const int WM_SYSKEYUP = 0x105;

		/// <summary>
		/// The WM_LBUTTONUP message is posted when the user releases the left mouse button 
		/// </summary>
		private const int WM_LBUTTONUP = 0x202;

		/// <summary>
		/// The WM_RBUTTONUP message is posted when the user releases the right mouse button 
		/// </summary>
		private const int WM_RBUTTONUP = 0x205;

		/// <summary>
		/// The WM_MBUTTONUP message is posted when the user releases the middle mouse button 
		/// </summary>
		private const int WM_MBUTTONUP = 0x208;

		/// <summary>
		/// The WM_XBUTTONUP message is posted when the user releases the first or second X mouse button
		/// </summary>
		private const int WM_XBUTTONUP = 0x020C;

		/// <summary>
		/// Stores the handle to the mouse hook procedure.
		/// </summary>
		private static int hMouseHook = 0;

		/// <summary>
		/// Stores the handle to the keyboard hook procedure.
		/// </summary>
		private static int hKeyboardHook = 0;

		/// <summary>
		/// Declare MouseHookProcedure as HookProc type.
		/// </summary>
		private static HookProc MouseHookProcedure;

		/// <summary>
		/// Declare KeyboardHookProcedure as HookProc type.
		/// </summary>
		private static HookProc KeyboardHookProcedure;

		/// <summary>
		/// The CallWndProc hook procedure is an application-defined or library-defined callback 
		/// function used with the SetWindowsHookEx function. The HOOKPROC type defines a pointer 
		/// to this callback function. CallWndProc is a placeholder for the application-defined 
		/// or library-defined function name.
		/// </summary>
		/// <param name="nCode">
		/// [in] Specifies whether the hook procedure must process the message. 
		/// If nCode is HC_ACTION, the hook procedure must process the message. 
		/// If nCode is less than zero, the hook procedure must pass the message to the 
		/// CallNextHookEx function without further processing and must return the 
		/// value returned by CallNextHookEx.
		/// </param>
		/// <param name="wParam">
		/// [in] Specifies whether the message was sent by the current thread. 
		/// If the message was sent by the current thread, it is nonzero; otherwise, it is zero. 
		/// </param>
		/// <param name="lParam">
		/// [in] Pointer to a CWPSTRUCT structure that contains details about the message. 
		/// </param>
		/// <returns>
		/// If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx. 
		/// If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx 
		/// and return the value it returns; otherwise, other applications that have installed WH_CALLWNDPROC 
		/// hooks will not receive hook notifications and may behave incorrectly as a result. If the hook 
		/// procedure does not call CallNextHookEx, the return value should be zero. 
		/// </returns>
		/// <remarks>
		/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/callwndproc.asp
		/// </remarks>
		private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

		/// <summary>
		/// The MOUSEHOOKSTRUCT structure contains information about a mouse event passed to a WH_MOUSE hook procedure, MouseProc. 
		/// </summary>
		/// <remarks>
		/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookstructures/cwpstruct.asp
		/// </remarks>
		[StructLayout(LayoutKind.Sequential)]
		private class MouseHookStruct
		{
			/// <summary>
			/// Specifies a POINT structure that contains the x- and y-coordinates of the cursor, in screen coordinates. 
			/// </summary>
			internal Win32Point pt;
			/// <summary>
			/// Handle to the window that will receive the mouse message corresponding to the mouse event. 
			/// </summary>
			internal int hwnd;
			/// <summary>
			/// Specifies the hit-test value. For a list of hit-test values, see the description of the WM_NCHITTEST message. 
			/// </summary>
			internal int wHitTestCode;
			/// <summary>
			/// Specifies extra information associated with the message. 
			/// </summary>
			internal int dwExtraInfo;
		}

		/// <summary>
		/// The MSLLHOOKSTRUCT structure contains information about a low-level keyboard input event. 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private class MouseLLHookStruct
		{
			/// <summary>
			/// Specifies a POINT structure that contains the x- and y-coordinates of the cursor, in screen coordinates. 
			/// </summary>
			internal Win32Point pt;
			/// <summary>
			/// If the message is WM_MOUSEWHEEL, the high-order word of this member is the wheel delta. 
			/// The low-order word is reserved. A positive value indicates that the wheel was rotated forward, 
			/// away from the user; a negative value indicates that the wheel was rotated backward, toward the user. 
			/// One wheel click is defined as WHEEL_DELTA, which is 120. 
			///If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP,
			/// or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released, 
			/// and the low-order word is reserved. This value can be one or more of the following values. Otherwise, mouseData is not used. 
			///XBUTTON1
			///The first X button was pressed or released.
			///XBUTTON2
			///The second X button was pressed or released.
			/// </summary>
			internal int mouseData;
			/// <summary>
			/// Specifies the event-injected flag. An application can use the following value to test the mouse flags. Value Purpose 
			///LLMHF_INJECTED Test the event-injected flag.  
			///0
			///Specifies whether the event was injected. The value is 1 if the event was injected; otherwise, it is 0.
			///1-15
			///Reserved.
			/// </summary>
			internal int flags;
			/// <summary>
			/// Specifies the time stamp for this message.
			/// </summary>
			internal int time;
			/// <summary>
			/// Specifies extra information associated with the message. 
			/// </summary>
			internal int dwExtraInfo;
		}


		/// <summary>
		/// The KBDLLHOOKSTRUCT structure contains information about a low-level keyboard input event. 
		/// </summary>
		/// <remarks>
		/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookstructures/cwpstruct.asp
		/// </remarks>
		[StructLayout(LayoutKind.Sequential)]
		private class KeyboardHookStruct
		{
			/// <summary>
			/// Specifies a virtual-key code. The code must be a value in the range 1 to 254. 
			/// </summary>
			internal int vkCode;
			/// <summary>
			/// Specifies a hardware scan code for the key. 
			/// </summary>
			internal int scanCode;
			/// <summary>
			/// Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.
			/// </summary>
			internal int flags;
			/// <summary>
			/// Specifies the time stamp for this message.
			/// </summary>
			internal int time;
			/// <summary>
			/// Specifies extra information associated with the message. 
			/// </summary>
			internal int dwExtraInfo;
		}

		/// <summary>
		/// The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain. 
		/// You would install a hook procedure to monitor the system for certain types of events. These events 
		/// are associated either with a specific thread or with all threads in the same desktop as the calling thread. 
		/// </summary>
		/// <param name="idHook">
		/// [in] Specifies the type of hook procedure to be installed. This parameter can be one of the following values.
		/// </param>
		/// <param name="lpfn">
		/// [in] Pointer to the hook procedure. If the dwThreadId parameter is zero or specifies the identifier of a 
		/// thread created by a different process, the lpfn parameter must point to a hook procedure in a dynamic-link 
		/// library (DLL). Otherwise, lpfn can point to a hook procedure in the code associated with the current process.
		/// </param>
		/// <param name="hMod">
		/// [in] Handle to the DLL containing the hook procedure pointed to by the lpfn parameter. 
		/// The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread created by 
		/// the current process and if the hook procedure is within the code associated with the current process. 
		/// </param>
		/// <param name="dwThreadId">
		/// [in] Specifies the identifier of the thread with which the hook procedure is to be associated. 
		/// If this parameter is zero, the hook procedure is associated with all existing threads running in the 
		/// same desktop as the calling thread. 
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is the handle to the hook procedure.
		/// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
		/// </returns>
		/// <remarks>
		/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
		/// </remarks>
		[DllImport("user32.dll", CharSet = CharSet.Auto,
		   CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		private static extern int SetWindowsHookEx(
			int idHook,
			HookProc lpfn,
			IntPtr hMod,
			int dwThreadId);

		/// <summary>
		/// The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx function. 
		/// </summary>
		/// <param name="idHook">
		/// [in] Handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx. 
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is nonzero.
		/// If the function fails, the return value is zero. To get extended error information, call GetLastError.
		/// </returns>
		/// <remarks>
		/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
		/// </remarks>
		[DllImport("user32.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		private static extern int UnhookWindowsHookEx(int idHook);

		/// <summary>
		/// The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain. 
		/// A hook procedure can call this function either before or after processing the hook information. 
		/// </summary>
		/// <param name="idHook">Ignored.</param>
		/// <param name="nCode">
		/// [in] Specifies the hook code passed to the current hook procedure. 
		/// The next hook procedure uses this code to determine how to process the hook information.
		/// </param>
		/// <param name="wParam">
		/// [in] Specifies the wParam value passed to the current hook procedure. 
		/// The meaning of this parameter depends on the type of hook associated with the current hook chain. 
		/// </param>
		/// <param name="lParam">
		/// [in] Specifies the lParam value passed to the current hook procedure. 
		/// The meaning of this parameter depends on the type of hook associated with the current hook chain. 
		/// </param>
		/// <returns>
		/// This value is returned by the next hook procedure in the chain. 
		/// The current hook procedure must also return this value. The meaning of the return value depends on the hook type. 
		/// For more information, see the descriptions of the individual hook procedures.
		/// </returns>
		/// <remarks>
		/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
		/// </remarks>
		[DllImport("user32.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.StdCall)]
		private static extern int CallNextHookEx(
			int idHook,
			int nCode,
			int wParam,
			IntPtr lParam);

		/// <summary>
		/// Installs both or one of mouse and/or keyboard hooks and starts raising events
		/// </summary>
		/// <param name="installMouseHook"><see langword="true"/> if mouse events must be monitored</param>
		/// <param name="installKeyboardHook"><see langword="true"/> if keyboard events must be monitored</param>
		/// <exception cref="Win32Exception">Any windows exception</exception>
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
					Marshal.GetHINSTANCE(
						Assembly.GetExecutingAssembly().GetModules()[0]),
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
					Marshal.GetHINSTANCE(
					Assembly.GetExecutingAssembly().GetModules()[0]),
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
		/// <param name="UninstallMouseHook"><b>true</b> if mouse hook must be uninstalled</param>
		/// <param name="uninstallKeyboardHook"><b>true</b> if keyboard hook must be uninstalled</param>
		/// <param name="throwExceptions"><b>true</b> if exceptions which occurred during uninstalling must be thrown</param>
		/// <exception cref="Win32Exception">Any windows problem.</exception>
		internal static void Stop(bool UninstallMouseHook, bool uninstallKeyboardHook, bool throwExceptions)
		{
			// If mouse hook set and must be uninstalled
			if (hMouseHook != 0 && UninstallMouseHook)
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

		/// <summary>
		/// A callback function which will be called every time a keyboard activity detected.
		/// </summary>
		/// <param name="nCode">
		/// [in] Specifies whether the hook procedure must process the message. 
		/// If nCode is HC_ACTION, the hook procedure must process the message. 
		/// If nCode is less than zero, the hook procedure must pass the message to the 
		/// CallNextHookEx function without further processing and must return the 
		/// value returned by CallNextHookEx.
		/// </param>
		/// <param name="wParam">
		/// [in] The identifier of the keyboard message.
		/// This parameter can be one of the following messages: WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP.
		/// </param>
		/// <param name="lParam">
		/// [in] Pointer to a CWPSTRUCT structure that contains details about the message. 
		/// </param>
		/// <returns>
		/// If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx. 
		/// If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx 
		/// and return the value it returns; otherwise, other applications that have installed WH_CALLWNDPROC 
		/// hooks will not receive hook notifications and may behave incorrectly as a result. If the hook 
		/// procedure does not call CallNextHookEx, the return value should be zero. 
		/// </returns>
		private static int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
		{
			// Indicates if any of underlying events set the Handled flag
			var handled = false;

			// It was ok and someone listens to events
			if ((nCode >= 0) && OnKeyboardKeyUp != null)
			{
				// Read structure KeyboardHookStruct at lParam
				var keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

				// Skip processing if the key is not in a transition state
				if ((keyboardHookStruct?.flags & 0b1000_0000) == 0)
				{
					return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
				}

				// Raise OnKeyboardKeyUp
				if (OnKeyboardKeyUp != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
				{
					var eventArgs = new KeyUpEventArgs(keyboardHookStruct?.vkCode);
					OnKeyboardKeyUp(null, eventArgs);
					handled |= eventArgs.Handled;
				}
			}

			// If event is marked as handled, do not forward to other listeners
			return handled ? 1 : CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
		}

		/// <summary>
		/// A callback function which will be called every time a mouse activity detected.
		/// </summary>
		/// <param name="nCode">
		/// [in] Specifies whether the hook procedure must process the message. 
		/// If nCode is HC_ACTION, the hook procedure must process the message. 
		/// If nCode is less than zero, the hook procedure must pass the message to the 
		/// CallNextHookEx function without further processing and must return the 
		/// value returned by CallNextHookEx.
		/// </param>
		/// <param name="wParam">
		/// [in] The identifier of the mouse message.
		/// This parameter can be one of the following messages: WM_LBUTTONDOWN, WM_LBUTTONUP,
		/// WM_MOUSEMOVE, WM_MOUSEWHEEL, WM_MOUSEHWHEEL, WM_RBUTTONDOWN, or WM_RBUTTONUP.
		/// </param>
		/// <param name="lParam">
		/// [in] Pointer to a CWPSTRUCT structure that contains details about the message. 
		/// </param>
		/// <returns>
		/// If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx. 
		/// If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx 
		/// and return the value it returns; otherwise, other applications that have installed WH_CALLWNDPROC 
		/// hooks will not receive hook notifications and may behave incorrectly as a result. If the hook 
		/// procedure does not call CallNextHookEx, the return value should be zero. 
		/// </returns>
		private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
		{
			// Indicates if any of underlying events set the Handled flag
			var handled = false;

			// If ok and someone listens to our events
			if ((nCode >= 0) && OnMouseButtonUp != null)
			{
				// Read structure MouseLLHookStruct at lParam
				int? virtualKeycode = null;

				switch (wParam)
				{
					case WM_LBUTTONUP:
						virtualKeycode = 0x01;
						break;
					case WM_RBUTTONUP:
						virtualKeycode = 0x02;
						break;
					case WM_MBUTTONUP:
						virtualKeycode = 0x04;
						break;
					case WM_XBUTTONUP:
						var mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));
						if (mouseHookStruct.mouseData == 0x10000) virtualKeycode = 0x05;
						else if (mouseHookStruct.mouseData == 0x20000) virtualKeycode = 0x06;
						break;
					default:
						return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
				}

				var eventArgs = new KeyUpEventArgs(virtualKeycode);
				OnMouseButtonUp(null, eventArgs);
				handled |= eventArgs.Handled;
			}

			// If event is marked as handled, do not forward to other listeners
			return handled ? 1 : CallNextHookEx(hMouseHook, nCode, wParam, lParam);
		}
		#endregion
	}

	internal class KeyUpEventArgs : EventArgs
	{
		internal bool Handled = false;

		internal int? Key;

		internal KeyUpEventArgs(int? key)
		{
			Key = key;
		}
	}
}
