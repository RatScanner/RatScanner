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
	/// <summary>
	/// How many tasks replaying a session would add to progress. Failed counts
	/// separately because it is stored as complete plus a flag, and the UI reads
	/// the two as different outcomes.
	/// </summary>
	public readonly record struct RecoveredProgress(int Finished, int Failed) {
		public int Total => Finished + Failed;
	}

	/// <summary>One past session's logs, newest first.</summary>
	public sealed record Session(DateTime StartedAt, string Folder) {
		/// <summary>Short label for the session, e.g. "2024-05-01 12:34".</summary>
		public string Label => StartedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

		/// <summary>What replaying this session would add to quest progress.</summary>
		public RecoveredProgress Recovered { get; init; }
	}

	private static readonly Regex FolderRegex = new(
		@"^log_(?<timestamp>\d{4}\.\d{2}\.\d{2}_\d{1,2}-\d{2}-\d{2})");

	/// <summary>
	/// The past sessions found in the logs folder, newest first. Empty when the
	/// game has never been run or the folder cannot be read.
	/// </summary>
	internal static List<Session> Sessions() {
		var logsFolder = EftLogs.LogsFolder;
		if (string.IsNullOrEmpty(logsFolder)) return [];

		try {
			var sessions = new List<Session>();
			var skipped = 0;

			foreach (var folder in Directory.GetDirectories(logsFolder)) {
				var name = Path.GetFileName(folder);

				if (!name.StartsWith("log_", StringComparison.OrdinalIgnoreCase)) continue;

				var match = FolderRegex.Match(name);
				if (!match.Success
					|| !DateTime.TryParseExact(match.Groups["timestamp"].Value,
						"yyyy.MM.dd_H-mm-ss", CultureInfo.InvariantCulture,
						DateTimeStyles.None, out var startedAt)) {
					skipped++;
					Logger.LogDebug($"Skipped unrecognised EFT log session folder: {name}");
					continue;
				}

				var session = new Session(startedAt, folder);
				sessions.Add(session with { Recovered = Preview(session) });
			}

			if (skipped > 0) Logger.LogInfo($"Skipped {skipped} EFT log folder(s) that could not be read as a session");

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

		// output.log repeats the other logs and can run to hundreds of megabytes
		// over a long session, so it is skipped: quest events are read from the
		// push-notifications log, which carries them in a far smaller file.
		var wanted = new (EftLogType Type, string Keyword)[] {
			(EftLogType.Application, "application"),
			(EftLogType.Notifications, "push-notifications"),
		};

		foreach (var file in Directory.GetFiles(session.Folder, "*.log")) {
			var name = Path.GetFileName(file);

			// Past sessions prefix the log name with the session timestamp, e.g.
			// "2026.10.04_21-57-57_1.1.5.1.47510 application_000.log", so the log
			// kind is identified by a keyword rather than the whole name.
			var kind = wanted.FirstOrDefault(w =>
				name.IndexOf(w.Keyword, StringComparison.OrdinalIgnoreCase) >= 0);
			if (kind.Keyword == null) continue;

			try {
				// ReadWrite so a log the game still holds open is not locked out.
				using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using var reader = new StreamReader(stream);

				var text = reader.ReadToEnd();
				if (result.TryGetValue(kind.Type, out var existing)) result[kind.Type] = existing + text;
				else result[kind.Type] = text;
			} catch (Exception e) {
				Logger.LogWarning($"Could not read past log {name} from {session.Folder}: {e.Message}");
			}
		}

		return result;
	}

	/// <summary>
	/// What replaying a session would recover, without recording any of it.
	/// </summary>
	internal static RecoveredProgress Preview(Session session) {
		var recovered = new RecoveredProgress(0, 0);

		foreach (var (type, data) in Read(session)) {
			if (type != EftLogType.Notifications) continue;
			var found = EftQuestTracker.CountProgress(data);
			recovered = new RecoveredProgress(
				recovered.Finished + found.Finished, recovered.Failed + found.Failed);
		}

		return recovered;
	}
}