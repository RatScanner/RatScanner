﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RatLib
{
	public static class GameWindowLocator
	{

		// Used to find the EscapeFromTarkov window to position the overlay
		[DllImport("user32.dll")]
		private static extern IntPtr FindWindow(string className, string windowName);
		[DllImport("user32.dll")]
		private static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);

		// Retrieve the BSG Config for the display size and mode
		static public DisplayConfig GetGameDisplayConfig()
		{
			// Load the DisplayConfig from AppData/Roaming/Battlestate Games
			// Were loading a file with an .ini extension, but its actually JSON ¯\_(ツ)_/¯
			string configPath = Environment.ExpandEnvironmentVariables(@"%AppData%\Battlestate Games\Escape From Tarkov\Settings\Graphics.ini");
			return new DisplayConfig();
		}

		// Return the location of the process with the EscapeFromTarkov title
		static public Rectangle GetWindowLocation()
		{
			// Used to find the area of the EFT game client to set up the overlay location and size
			IntPtr hwnd = FindWindow("UnityWndClass", "EscapeFromTarkov");

			// Check if we found the EFT window
			if (hwnd != IntPtr.Zero)
			{
				// Nonzero intptr means we found it
				Rectangle tempRect;
				GetWindowRect(hwnd, out tempRect);

				// We do this because otherwise the rectangle generated by GetWindowRect sets the right hand size of the rectangle as the width & same thing for height
				Rectangle rect = new Rectangle(new Point(tempRect.X, tempRect.Y), new Size(Math.Abs(tempRect.Width - tempRect.X), Math.Abs(tempRect.Height - tempRect.Y) ));

				Debug.WriteLine($"Found the EFT Window at {rect.X}, {rect.Y}, {rect.Width}, {rect.Height}");

				return rect;
			}
			else
			{
				throw new Exception("Unable to find the EscapeFromTarkov window");
			}
		}

		public class DisplayConfig
		{
			public DisplayConfig()
			{

			}
		}
	}
}
