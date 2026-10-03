using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Timer = System.Threading.Timer;

namespace RatScanner;

/// <summary>Which of the game's log files a chunk came from.</summary>
internal enum EftLogType {
	Application,
	Notifications,
	Output,
}

/// <summary>
/// Follows the game's log files and hands newly appended text to a callback.
/// </summary>
/// <remarks>
/// Nothing here throws at the caller. A log format change, a deleted file or a
/// permission problem stops that one file from being read and is logged once; the
/// rest of RatScanner is unaffected.
/// </remarks>
internal sealed class EftLogMonitor : IDisposable {
	/// <summary>Raised on a background thread for each new chunk of log text.</summary>
	internal event Action<EftLogType, string>? DataReceived;

	private readonly object _gate = new();
	private readonly Dictionary<EftLogType, long> _readBytes = [];
	private readonly HashSet<string> _reportedFailures = [];
	private FileSystemWatcher? _watcher;
	private Timer? _pollTimer;
	private string _logsFolder = "";
	private bool _stopped;

	/// <summary>True once a logs folder was found and watching began.</summary>
	internal bool IsWatching { get; private set; }

	internal void Start() {
		var folder = EftLogs.LogsFolder;
		if (string.IsNullOrEmpty(folder)) {
			Logger.LogWarning("EFT log tracking is on but no logs folder could be found.");
			return;
		}

		lock (_gate) {
			if (_stopped || IsWatching) return;
			_logsFolder = folder;
			IsWatching = true;
		}

		Logger.LogInfo($"Following EFT logs in {folder}");

		try {
			_watcher = new FileSystemWatcher {
				Filter = "*.log",
				IncludeSubdirectories = true,
				Path = folder,
			};
			_watcher.Created += OnFileCreated;
			_watcher.Renamed += OnFileCreated;
			_watcher.Error += OnWatcherError;
			_watcher.EnableRaisingEvents = true;
		} catch (Exception e) {
			// The folder watcher is an optimisation; polling below still works.
			Logger.LogWarning($"Could not watch the EFT logs folder, falling back to polling: {e.Message}");
		}

		// The game rolls logs into a dated subfolder, so poll as well as watch to
		// pick up a new folder's files even when the watcher misses the creation.
		_pollTimer = new Timer(_ => Poll(), null, 0, 2000);

		Poll();
	}

	public void Dispose() {
		lock (_gate) {
			if (_stopped) return;
			_stopped = true;
			IsWatching = false;
		}

		if (_watcher != null) {
			_watcher.EnableRaisingEvents = false;
			_watcher.Created -= OnFileCreated;
			_watcher.Renamed -= OnFileCreated;
			_watcher.Error -= OnWatcherError;
			_watcher.Dispose();
			_watcher = null;
		}

		_pollTimer?.Dispose();
		_pollTimer = null;
	}

	private void OnFileCreated(object sender, FileSystemEventArgs e) => Poll();

	private void OnWatcherError(object sender, ErrorEventArgs e) {
		Logger.LogWarning($"The EFT logs folder watcher reported an error: {e.GetException().Message}");
	}

	/// <summary>
	/// Attaches to any log file we have not seen and reads whatever has been
	/// appended since last time. A file already being read is not read again, so
	/// each line is dispatched exactly once.
	/// </summary>
	private void Poll() {
		string folder;
		lock (_gate) {
			if (_stopped || !IsWatching) return;
			folder = _logsFolder;
		}

		if (!Directory.Exists(folder)) return;

		IEnumerable<string> files;
		try {
			files = Directory.EnumerateFiles(folder, "*.log", SearchOption.AllDirectories);
		} catch (Exception e) {
			ReportOnce(folder, e.Message);
			return;
		}

		foreach (var file in files) {
			var type = Classify(file);
			if (type == null) continue;

			try {
				Read(file, type.Value);
			} catch (Exception e) {
				ReportOnce(file, e.Message);
			}
		}
	}

	private void Read(string path, EftLogType type) {
		long alreadyRead;
		lock (_gate) {
			if (_stopped) return;
			_readBytes.TryGetValue(type, out alreadyRead);
		}

		using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

		// A file shorter than our mark means it was rotated and truncated, so
		// start over rather than seeking past the end.
		if (alreadyRead > stream.Length) alreadyRead = 0;
		if (stream.Length == alreadyRead) return;

		stream.Seek(alreadyRead, SeekOrigin.Begin);

		using var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen: true);
		var text = reader.ReadToEnd();

		long position;
		lock (_gate) {
			position = stream.Position;
			// Only advance the mark to what was actually consumed, so a partial
			// read resumes at the right byte next time.
			_readBytes[type] = position;
		}

		if (string.IsNullOrEmpty(text)) return;

		try {
			DataReceived?.Invoke(type, text);
		} catch (Exception e) {
			// A listener that trips over an unexpected line must not stop the feed.
			Logger.LogWarning($"Failed to handle EFT {type} log data: {e.Message}");
		}
	}

	/// <summary>
	/// Warns once per path so a persistently missing folder does not fill the log.
	/// </summary>
	private void ReportOnce(string key, string message) {
		lock (_gate) {
			if (!_reportedFailures.Add(key)) return;
		}

		Logger.LogWarning($"Could not read EFT logs at {key}: {message}");
	}

	private static EftLogType? Classify(string path) {
		var name = Path.GetFileName(path);

		if (name.EndsWith("application.log", StringComparison.OrdinalIgnoreCase)
			|| name.EndsWith("application_000.log", StringComparison.OrdinalIgnoreCase)) {
			return EftLogType.Application;
		}

		if (name.EndsWith("notifications.log", StringComparison.OrdinalIgnoreCase)
			|| name.EndsWith("notifications_000.log", StringComparison.OrdinalIgnoreCase)) {
			return EftLogType.Notifications;
		}

		if (name.EndsWith("output.log", StringComparison.OrdinalIgnoreCase)
			|| name.EndsWith("output_000.log", StringComparison.OrdinalIgnoreCase)) {
			return EftLogType.Output;
		}

		return null;
	}
}