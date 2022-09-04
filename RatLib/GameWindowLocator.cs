﻿using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RatLib;

public static class GameWindowLocator
{

	// Used to find the EscapeFromTarkov window to position the overlay
	[DllImport("user32.dll")]
	private static extern IntPtr FindWindow(string className, string windowName);

	[DllImport("user32.dll")]
	private static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);

	// Retrieve the BSG Config for the display size and mode
	public static Rectangle GetGameDisplayConfig()
	{
		// Load the DisplayConfig from AppData/Roaming/Battlestate Games
		// Were loading a file with an .ini extension, but its actually JSON ¯\_(ツ)_/¯
		var configPath = Environment.ExpandEnvironmentVariables(@"%AppData%\Battlestate Games\Escape From Tarkov\Settings\Graphics.ini");
		try
		{
			using var file = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var reader = new StreamReader(file, Encoding.UTF8);

			var json = JObject.Parse(reader.ReadToEnd());

			var activeDisplay = json["DisplaySettings"]["Display"].ToObject<int>();
			var windowRes = json["Stored"][activeDisplay.ToString()]["WindowResolution"];
			var width = windowRes["Width"].ToObject<int>();
			var height = windowRes["Height"].ToObject<int>();

			// TODO (Once all projects are merged to get access to the WinForms lib)
			// Use System.Windows.Forms.Screen.AllScreens to determine the correct x/y value of the selected screen

			return new Rectangle(0, 0, width, height);
		}
		catch (FileNotFoundException e)
		{
			throw new Exception("Unable to find Escape From Tarkov graphic settings file.", e);
		}
	}

	// Return the location of the process with the EscapeFromTarkov title
	public static Rectangle GetWindowLocation()
	{
		// Used to find the area of the EFT game client to set up the overlay location and size
		// NOTE className can be nulled in case of frequent changes
		var hwnd = FindWindow("UnityWndClass", "EscapeFromTarkov");

		// Check if we found the EFT window
		if (hwnd == IntPtr.Zero)
		{
			// TODO Log.Warning("Unable to find the EscapeFromTarkov window");
			Debug.WriteLine("Could not find the EscapeFromTarkov window");
			return GetGameDisplayConfig();
		}

		// Nonzero IntPtr means we found it
		GetWindowRect(hwnd, out var tempRect);

		// We do this because otherwise the rectangle generated by GetWindowRect sets the right hand size of the rectangle as the width & same thing for height
		var rect = new Rectangle(new Point(tempRect.X, tempRect.Y), new Size(Math.Abs(tempRect.Width - tempRect.X), Math.Abs(tempRect.Height - tempRect.Y)));

		Debug.WriteLine($"Found it at {rect.ToString()}");

		return rect;
	}
}
