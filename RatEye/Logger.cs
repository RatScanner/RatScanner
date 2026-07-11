using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using OpenCvSharp;

namespace RatEye
{
	/// <summary>
	/// Class for logging events and control flow of RatEye
	/// </summary>
	internal static class Logger
	{
		private static List<string> _backlog = new();

		internal static void LogDebug(string message, Exception e)
		{
			LogDebug(message + "\nException: " + e);
		}

		internal static void LogDebug(string message)
		{
			if (Config.LogDebug) AppendToLog("[Debug] " + message);
		}

		internal static void LogDebugBitmap(Bitmap bitmap, string fileName = "bitmap")
		{
			if (Config.LogDebug)
			{
				bitmap.Save(GetUniquePath(Config.Path.Debug, fileName, ".png"));
			}
		}

		internal static void LogDebugMat(OpenCvSharp.Mat mat, string fileName = "mat")
		{
			if (!Config.LogDebug) return;

			var tmp = mat;
			if (mat.Type() == MatType.CV_32FC1)
			{
				tmp = new Mat(mat.Size(), MatType.CV_8UC1);
				mat.ConvertTo(tmp, MatType.CV_8UC1, 255);
			}
			tmp.SaveImage(GetUniquePath(Config.Path.Debug, fileName, ".png"));
		}

		private static string GetUniquePath(string basePath, string fileName, string extension)
		{
			fileName = fileName.Replace(' ', '_');

			var index = 0;
			var uniquePath = Path.Combine(basePath, fileName + "(" + index + ")" + extension);

			while (File.Exists(uniquePath))
			{
				index += 1;
				uniquePath = Path.Combine(basePath, fileName + "(" + index + ")" + extension);
			}

			Directory.CreateDirectory(Path.GetDirectoryName(uniquePath));
			return uniquePath;
		}

		private static void AppendToLog(string content)
		{
			ProcessBacklog();

			var prefix = "[" + DateTime.UtcNow.ToUniversalTime().TimeOfDay + "] > ";

			try { AppendToLogRaw(prefix + content + "\n"); }
			catch (Exception e)
			{
				_backlog.Add(prefix + "Could not write to log file\n" + e + "\n");
				_backlog.Add(prefix + content + "\n");
				Thread.Sleep(250);
				ProcessBacklog();
			}
		}

		private static void AppendToLogRaw(string text)
		{
			System.Diagnostics.Debug.WriteLine(text);
			File.AppendAllText(Config.Path.LogFile, text, Encoding.UTF8);
		}

		private static void ProcessBacklog()
		{
			var newBacklog = new List<string>();

			foreach (var text in _backlog)
			{
				try { AppendToLogRaw(text); }
				catch { newBacklog.Add(text); }
			}

			_backlog = newBacklog;
		}
	}
}
