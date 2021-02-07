using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RatScanner
{
	internal static class Logger
	{
		private static readonly object SyncObject = new object();

		private static readonly Queue<string> Backlog = new Queue<string>();

		internal static void LogInfo(string message)
		{
			AppendToLog("[Info]  " + message);
		}

		internal static void LogWarning(string message, Exception e = null)
		{
			AppendToLog("[Warning] " + message);
			if (e != null) AppendToLog(e.ToString());
		}

		internal static void LogError(string message, Exception e = null)
		{
			// Log the error
			var logMessage = "[Error] " + message;
			var divider = new string('-', 20);
			if (e != null) logMessage += $"\n {divider} \n {e}";
			else logMessage += $"\n {divider} \n {Environment.StackTrace}";
			AppendToLog(logMessage);

			// Setup info box
			var title = "RatScanner " + RatConfig.Version;

			// Ask to open FAQ
			var faqBoxMessage = message + "\n\nThe FAQ will probably help with that.\nDo you want to open it now?";
			var faqBoxResult = MessageBox.Show(faqBoxMessage, title, MessageBoxButton.YesNo, MessageBoxImage.Error);
			if (faqBoxResult == MessageBoxResult.Yes) OpenFAQ(message);

			// Ask for git issue creation
			var gitBoxMessage = "Would you like to create a issue on GitHub?";
			var gitBoxResult = MessageBox.Show(gitBoxMessage, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (gitBoxResult == MessageBoxResult.Yes) CreateGitHubIssue(message, e);

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

		internal static void LogDebugBitmap(Bitmap bitmap, string fileName = "bitmap")
		{
			if (RatConfig.LogDebug)
			{
				bitmap.Save(GetUniquePath(RatConfig.Paths.Debug, fileName, ".png"));
			}
		}

		internal static void LogDebug(string message)
		{
			if (RatConfig.LogDebug) AppendToLog("[Debug] " + message);
		}

		internal static void ShowMessage(string message, string title = null)
		{
			MessageBox.Show(message, title ?? "Rat Scanner " + RatConfig.Version, MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private static string GetUniquePath(string basePath, string fileName, string extension)
		{
			fileName = fileName.Replace(' ', '_');

			var index = 0;
			var uniquePath = Path.Combine(basePath, fileName + index + extension);

			while (File.Exists(uniquePath))
			{
				index += 1;
				uniquePath = Path.Combine(basePath, fileName + index + extension);
			}

			Directory.CreateDirectory(Path.GetDirectoryName(uniquePath));
			return uniquePath;
		}

		private static void AppendToLog(string content)
		{
			var text = "[" + DateTime.UtcNow.ToUniversalTime().TimeOfDay + "] > " + content + "\n";
			Backlog.Enqueue(text);
			Task.Run((() => ProcessBacklog()));
		}

		private static void AppendToLogRaw(string text)
		{
			Debug.WriteLine(text);
			File.AppendAllText(RatConfig.Paths.LogFile, text, Encoding.UTF8);
		}

		private static void ProcessBacklog()
		{
			lock (SyncObject)
			{
				for (var i = 0; i < Backlog.Count; i++)
				{
					AppendToLogRaw(Backlog.Dequeue());
				}
			}
		}

		internal static void Clear()
		{
			File.Delete(RatConfig.Paths.LogFile);
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
			if (!Directory.Exists(RatConfig.Paths.Debug)) return;

			var files = Directory.GetFiles(RatConfig.Paths.Debug, "*.png");
			foreach (var file in files)
			{
				try
				{
					File.Delete(file);
				}
				catch (Exception)
				{
					Logger.LogDebug("Exception while deleting debug mats.");
				}
			}
		}

		private static void OpenFAQ(string message)
		{
			// Remove everything after ':' which is commonly a path
			message = message.Split(':')[0];
			var url = ApiManager.GetResource(ApiManager.ResourceType.FAQ);
			url += "#:~:text=" + WebUtility.HtmlEncode(message);
			OpenURL(url);
		}

		private static void CreateGitHubIssue(string message, Exception e)
		{
			var body = "**Error**\n" + message + "\n";
			if (e != null)
			{
				body += "```\n" + LimitLength(e.ToString(), 1000) + "\n```\n";
			}

			body += "<details>\n<summary>Log</summary>\n\n```\n";
			body += LimitLength(ReadAll(), 3000);
			body += "\n```\n</details>";

			var title = message;

			var labels = "bug";

			var url = ApiManager.GetResource(ApiManager.ResourceType.Github);
			url += "/issues/new";
			url += "?body=" + WebUtility.UrlEncode(body);
			url += "&title=" + WebUtility.UrlEncode(title);
			url += "&labels=" + WebUtility.UrlEncode(labels);

			OpenURL(url);
		}

		private static string LimitLength(string input, int length)
		{
			return input.Substring(0, input.Length > length ? length : input.Length);
		}

		private static void OpenURL(string url)
		{
			var psi = new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			};
			Process.Start(psi);
		}

		private static string ReadAll()
		{
			return File.ReadAllText(RatConfig.Paths.LogFile, Encoding.UTF8);
		}
	}
}
