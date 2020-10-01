using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;

namespace RatScanner
{
	internal class Logger
	{
		private const string LogFile = "Log.txt";
		private static bool writeMutex = false;

		internal static void LogInfo(string message)
		{
			AppendToLog("[Info]  " + message);
		}

		internal static void LogWarning(string message, Exception e = null)
		{
			AppendToLog("[Warning] " + message);
			AppendToLog(e == null ? Environment.StackTrace : e.ToString());
		}

		internal static void LogError(string message, Exception e = null)
		{
			// Log the error
			var logMessage = "[Error] " + message;
			var divider = new string('-', 20);
			if (e != null) logMessage += $"\n {divider} \n {e}";
			else logMessage += $"\n {divider} \n {Environment.StackTrace}";
			AppendToLog(logMessage);

			// Ask for git issue creation
			var title = "RatScanner " + RatConfig.Version;
			var msgBoxMessage = message + "\n\nWould you like to report this on GitHub?";
			var msgBoxResult = MessageBox.Show(msgBoxMessage, title, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
			if (msgBoxResult == MessageBoxResult.Yes) CreateGitHubIssue(message, e);

			// Exit after error is handled
			Environment.Exit(0);
		}

		internal static void LogMat(OpenCvSharp.Mat mat, string fileName = "mat")
		{
			mat.SaveImage(GetUniquePath(RatConfig.Paths.Data, fileName, ".png"));
		}

		internal static void LogDebugMat(OpenCvSharp.Mat mat, string fileName = "mat")
		{
			if (RatConfig.LogDebug)
			{
				mat.SaveImage(GetUniquePath(RatConfig.Paths.Debug, fileName, ".png"));
			}
		}

		internal static void LogDebug(string message)
		{
			if (RatConfig.LogDebug) AppendToLog("[Debug] " + message);
		}

		private static string GetUniquePath(string basePath, string fileName, string extension)
		{
			fileName = fileName.Replace(' ', '_');

			var index = 0;
			var uniquePath = Path.Combine(basePath, fileName + index + extension);

			while (File.Exists(uniquePath)) index += 1;

			return Path.Combine(RatConfig.Paths.Debug, fileName + index + extension);
		}

		private static void AppendToLog(string content)
		{
			var text = "[" + DateTime.UtcNow.ToUniversalTime().TimeOfDay + "] > " + content + "\n";

			Debug.WriteLine(text);

			if (!writeMutex)
			{
				writeMutex = true;
				File.AppendAllText(LogFile, text, Encoding.UTF8);
				writeMutex = false;
			}
		}

		internal static void Clear()
		{
			File.Delete(LogFile);
		}

		internal static void ClearMats(string pattern = "*.png")
		{
			var files = Directory.GetFiles(RatConfig.Paths.Data, pattern);
			foreach (var file in files)
			{
				File.Delete(file);
			}
		}

		internal static void ClearDebugMats()
		{
			if(!Directory.Exists(RatConfig.Paths.Debug)) return;

			var files = Directory.GetFiles(RatConfig.Paths.Debug, "*.png");
			foreach (var file in files)
			{
				File.Delete(file);
			}
		}

		private static void CreateGitHubIssue(string message, Exception e)
		{
			var body = "**Error**\n" + message + "\n";
			if (e != null) body += "```\n" + e + "\n```\n";
			body += "<details>\n<summary>Log</summary>\n\n```\n" + ReadAll() + "```\n</details>";

			var title = message;

			var labels = "bug";

			var url = ApiManager.GetResource(ApiManager.ResourceType.Github);
			url += "/issues/new";
			url += "?body=" + WebUtility.UrlEncode(body);
			url += "&title=" + WebUtility.UrlEncode(title);
			url += "&labels=" + WebUtility.UrlEncode(labels);

			var psi = new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			};
			Process.Start(psi);
		}

		private static string ReadAll()
		{
			return File.ReadAllText(LogFile, Encoding.UTF8);
		}
	}
}
