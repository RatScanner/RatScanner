using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RatScanner;

/// <summary>
/// Reads logs the game wrote in previous sessions, so quest progress from raids
/// that already happened is picked up instead of only tracking from the moment
/// the app was started.
///
/// The game rolls its logs into a folder per session, named
/// <c>log_yyyy.MM.dd_H-mm-ss</c>. Each folder holds the same file names the live
/// monitor tails, so the text is handed to the same parsers rather than being
/// interpreted again here.
/// </summary>
public static class EftLogHistory {
	private static readonly Regex FolderRegex = new(@"^log_(?<timestamp>\d{4}\.\d{2}\.\d{2}_\d{2}-\d{2}-\d{2})$");

	/// <summary>One past session's logs, newest first.</summary>
	public sealed record Session(DateTime StartedAt, string Folder) {
		/// <summary>Short label for the session, e.g. "2024-05-01 12:34".</summary>
		public string Label => StartedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// The past sessions found in the logs folder, newest first. Empty when the
	/// game has never been run or the folder cannot be read.
	/// </summary>
	internal static List<Session> Sessions() {
		var logsFolder = EftLogs.LogsFolder;
		if (string.IsNullOrEmpty(logsFolder)) return [];

		try {
			var sessions = new List<Session>();

			foreach (var folder in Directory.GetDirectories(logsFolder)) {
				var name = Path.GetFileName(folder);
				var match = FolderRegex.Match(name);
				if (!match.Success) continue;

				if (!DateTime.TryParseExact(match.Groups["timestamp"].Value,
					"yyyy.MM.dd_H-mm-ss", CultureInfo.InvariantCulture,
					DateTimeStyles.None, out var startedAt)) {
					continue;
				}

				sessions.Add(new Session(startedAt, folder));
			}

			return [.. sessions.OrderByDescending(s => s.StartedAt)];
		} catch (Exception e) {
			Logger.LogWarning($"Could not list past EFT log sessions: {e.Message}");
			return [];
		}
	}

	/// <summary>
	/// The logs in a session, keyed by the parser that should read them.
	/// </summary>
	internal static Dictionary<EftLogType, string> Read(Session session) {
		var result = new Dictionary<EftLogType, string>();
		if (session == null || !Directory.Exists(session.Folder)) return result;

		// Read the small logs only. output.log repeats the other two and can run
		// to hundreds of megabytes over a long session, so reading it would cost
		// a lot of time and memory for text nothing consumes.
		foreach (var (type, name) in new (EftLogType Type, string Name)[] {
			(EftLogType.Application, "application.log"),
			(EftLogType.Application, "application_000.log"),
			(EftLogType.Notifications, "notifications.log"),
			(EftLogType.Notifications, "notifications_000.log"),
		}) {
			var path = Path.Combine(session.Folder, name);
			if (!File.Exists(path)) continue;

			try {
				// ReadWrite so a log the game still holds open for the current
				// session is not locked out.
				using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using var reader = new StreamReader(stream);

				var text = reader.ReadToEnd();
				if (result.TryGetValue(type, out var existing)) result[type] = existing + text;
				else result[type] = text;
			} catch (Exception e) {
				Logger.LogWarning($"Could not read past log {name} from {session.Folder}: {e.Message}");
			}
		}

		return result;
	}
}