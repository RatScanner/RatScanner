using RatScanner.TarkovDev.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RatScanner;

/// <summary>
/// Watches the folder the game drops screenshots into and turns their file names
/// into a player position, so the map can show where the player is standing and
/// which way they are facing.
///
/// The game encodes the position and the player's rotation into the screenshot
/// name, so no screenshot is ever opened or decoded.
/// </summary>
internal sealed class EftPlayerPosition : IDisposable {
	// "2024-05-01[12-34]_12.34, 56.78, 90.12_0.00000, 0.70711, 0.00000, 0.70711 (1).png"
	private static readonly Regex NameRegex = new(
		@"\d{4}-\d{2}-\d{2}\[\d{2}-\d{2}\]_?(?<position>.+) \(\d\)\.png");

	// The coordinates and the quaternion, as written in the file name.
	private static readonly Regex PositionRegex = new(
		@"(?<x>-?[\d]+\.[\d]{2}), (?<y>-?[\d]+\.[\d]{2}), (?<z>-?[\d]+\.[\d]{2})" +
		@"_?(?<rx>-?[\d.]{1}\.[\d]{1,5}), (?<ry>-?[\d.]{1}\.[\d]{1,5}), (?<rz>-?[\d.]{1}\.[\d]{1,5}), (?<rw>-?[\d.]{1}\.[\d]{1,5})");

	private static readonly TimeSpan SettleDelay = TimeSpan.FromSeconds(1);

	private readonly object _gate = new();

	private FileSystemWatcher? _watcher;

	/// <summary>Latest position, or null until the first screenshot is seen.</summary>
	private WorldPosition? _position;

	/// <summary>Latest facing in degrees, or null until the first screenshot is seen.</summary>
	private double? _rotation;

	/// <summary>
	/// Raised when a new position arrives, on a background thread.
	/// </summary>
	public event Action? PositionChanged;

	/// <summary>
	/// Starts watching. Does nothing when the folder cannot be found, which is
	/// the normal case until the player has taken a screenshot.
	/// </summary>
	public void Start() {
		var folder = EftLogs.ScreenshotsFolder;
		if (string.IsNullOrEmpty(folder)) {
			Logger.LogWarning("EFT screenshots folder not found; player position tracking is off.");
			return;
		}

		try {
			var watcher = new FileSystemWatcher {
				Path = folder,
				Filter = "*.png",
				NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
				IncludeSubdirectories = true,
			};

			watcher.Created += OnCreated;
			watcher.Error += OnError;
			watcher.EnableRaisingEvents = true;

			lock (_gate) {
				_watcher = watcher;
			}

			Logger.LogInfo($"Watching EFT screenshots for player position: {folder}");

			// The folder may already hold screenshots from this session, e.g. if
			// tracking was enabled after the player had been playing.
			foreach (var file in RecentScreenshots(folder)) Handle(file);
		} catch (Exception e) {
			Logger.LogWarning($"Could not watch the EFT screenshots folder: {e.Message}");
		}
	}

	/// <summary>
	/// The screenshots already in the folder, newest first, capped so enabling
	/// tracking on an old folder does not replay a whole raid.
	/// </summary>
	private static string[] RecentScreenshots(string folder) {
		try {
			return new DirectoryInfo(folder)
				.GetFiles("*.png", SearchOption.AllDirectories)
				.OrderByDescending(f => f.LastWriteTimeUtc)
				.Take(5)
				.Select(f => f.Name)
				.ToArray();
		} catch (Exception e) {
			Logger.LogWarning($"Could not list existing EFT screenshots: {e.Message}");
			return [];
		}
	}

	private void OnCreated(object sender, FileSystemEventArgs e) {
		var name = e.Name;
		if (string.IsNullOrEmpty(name)) return;

		// The file is still being written, so a read now can catch a half-written
		// name. Nothing is read from it, but the watcher still reports it early.
		_ = Task.Run(async () => {
			await Task.Delay(SettleDelay).ConfigureAwait(false);
			Handle(name);
		});
	}

	private void OnError(object sender, ErrorEventArgs e) {
		Logger.LogWarning($"EFT screenshot watcher stopped: {e.GetException().Message}");
	}

	private void Handle(string name) {
		try {
			var match = NameRegex.Match(name);
			if (!match.Success) return;

			var parts = PositionRegex.Match(match.Groups["position"].Value);
			if (!parts.Success) return;

			var position = new WorldPosition {
				X = double.Parse(parts.Groups["x"].Value, CultureInfo.InvariantCulture),
				Y = double.Parse(parts.Groups["y"].Value, CultureInfo.InvariantCulture),
				Z = double.Parse(parts.Groups["z"].Value, CultureInfo.InvariantCulture),
			};

			var rotation = Yaw(
				float.Parse(parts.Groups["rx"].Value, CultureInfo.InvariantCulture),
				float.Parse(parts.Groups["ry"].Value, CultureInfo.InvariantCulture),
				float.Parse(parts.Groups["rz"].Value, CultureInfo.InvariantCulture),
				float.Parse(parts.Groups["rw"].Value, CultureInfo.InvariantCulture));

			lock (_gate) {
				_position = position;
				_rotation = rotation;
			}

			PositionChanged?.Invoke();
		} catch (Exception e) {
			// A screenshot name that does not parse is not worth surfacing: the
			// player takes hundreds of them and some are not positional.
			Logger.LogDebug($"Ignored EFT screenshot {name}: {e.Message}");
		}
	}

	/// <summary>
	/// The player's facing in degrees, from the rotation quaternion the game
	/// writes into the file name.
	/// </summary>
	private static double Yaw(float x, float y, float z, float w) {
		var sin = 2.0f * (w * y + x * z);
		var cos = 1.0f - 2.0f * (y * y + z * z);
		return Math.Atan2(sin, cos) * (180.0 / Math.PI);
	}

	/// <summary>
	/// The last position seen, or null when there is none. Only worth reading
	/// while a raid is running: the file names carry no timestamp to age out.
	/// </summary>
	public WorldPosition? CurrentPosition {
		get {
			lock (_gate) {
				return _position;
			}
		}
	}

	/// <summary>The last facing seen in degrees, or null when there is none.</summary>
	public double? CurrentRotation {
		get {
			lock (_gate) {
				return _rotation;
			}
		}
	}

	public void Dispose() {
		FileSystemWatcher? watcher;
		lock (_gate) {
			watcher = _watcher;
			_watcher = null;
		}

		if (watcher == null) return;

		watcher.EnableRaisingEvents = false;
		watcher.Created -= OnCreated;
		watcher.Error -= OnError;
		watcher.Dispose();
	}
}