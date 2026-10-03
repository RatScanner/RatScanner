using RatScanner.TarkovDev.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RatScanner;

/// <summary>
/// Follows a raid through the application log so the user can be told what is
/// happening, and so the map they are on is known for the position marker.
/// </summary>
/// <remarks>
/// Every message is reported through <see cref="Logger.Notify"/>, which the UI
/// shows. With tracking off nothing is parsed at all.
/// </remarks>
internal static class EftRaidLifecycle {
	private static readonly Regex ScenePathRegex = new(@"scene preset path:(?<scenePath>maps\/[a-zA-Z0-9_]+\.bundle)", RegexOptions.IgnoreCase);

	private static readonly object Gate = new();

	private static string _mapId = "";
	private static bool _raidActive;

	/// <summary>
	/// The map the last raid was on, or empty when no raid has been seen. Falls
	/// back to the configured map so the position marker still has somewhere to
	/// go when no raid was tracked.
	/// </summary>
	internal static string CurrentMapId {
		get {
			lock (Gate) {
				if (!string.IsNullOrEmpty(_mapId)) return _mapId;
			}

			return RatConfig.LogTracking.PositionMapFallback;
		}
	}

	/// <summary>Whether a raid is currently in progress according to the log.</summary>
	internal static bool IsRaidActive {
		get {
			lock (Gate) {
				return _raidActive;
			}
		}
	}

	internal static void Consume(string chunk) {
		if (string.IsNullOrEmpty(chunk)) return;

		MatchCollection lines;
		try {
			lines = EftLogLine.Matches(chunk);
		} catch (Exception e) {
			Logger.LogWarning($"Could not parse the EFT application log: {e.Message}");
			return;
		}

		foreach (Match line in lines) {
			try {
				var message = line.Groups["message"].Value;

				if (message.Contains("scene preset path:", StringComparison.OrdinalIgnoreCase)) {
					OnSceneLoaded(message);
				} else if (message.Contains("GameStarting", StringComparison.OrdinalIgnoreCase)) {
					OnRaidStarting();
				} else if (message.Contains("GameStarted", StringComparison.OrdinalIgnoreCase)) {
					OnRaidStarted();
				} else if (message.Contains("TRACE-NetworkGameCreate profileStatus", StringComparison.OrdinalIgnoreCase)) {
					OnMatchFound(message);
				} else if (message.Contains("Network game matching aborted", StringComparison.OrdinalIgnoreCase)
					|| message.Contains("Network game matching cancelled", StringComparison.OrdinalIgnoreCase)) {
					OnMatchingAborted();
				} else if (message.Contains("UserMatchOver", StringComparison.OrdinalIgnoreCase)) {
					OnRaidExited();
				} else if (message.Contains("Init: pstrGameVersion:", StringComparison.OrdinalIgnoreCase)) {
					OnRaidEnded();
				}
			} catch (Exception e) {
				Logger.LogWarning($"Skipped an unreadable EFT log line: {e.Message}");
			}
		}
	}

	private static void OnSceneLoaded(string message) {
		var match = ScenePathRegex.Match(message);
		if (!match.Success) return;

		var scenePath = match.Groups["scenePath"].Value;
		var map = TarkovDevAPI.GetMaps().FirstOrDefault(m => m.ScenePath == scenePath);
		if (map == null) return;

		lock (Gate) {
			_mapId = map.Id;
		}

		Logger.LogInfo($"EFT loading {map.Name}");
	}

	private static void OnMatchFound(string message) {
		// The location arrives on the same line as the profile status, which is
		// the earliest point the map is known by name rather than scene path.
		var nameId = Regex.Match(message, @"Location: (?<map>[^,\s]+)").Groups["map"].Value;
		if (!string.IsNullOrEmpty(nameId)) {
			var map = TarkovDevAPI.GetMaps().FirstOrDefault(m => m.NameId == nameId);
			if (map != null) {
				lock (Gate) {
					_mapId = map.Id;
				}
			}
		}

		var mapName = MapName();
		Logger.Notify(Logger.NotifyLevel.Info,
			string.IsNullOrEmpty(mapName)
				? LocalizationService.Format("LogTrackingRaidMatchFound")
				: LocalizationService.Format("LogTrackingRaidMatchFoundOn", mapName));
	}

	private static void OnRaidStarting() {
		var mapName = MapName();
		Logger.Notify(Logger.NotifyLevel.Info,
			string.IsNullOrEmpty(mapName)
				? LocalizationService.Format("LogTrackingRaidStarting")
				: LocalizationService.Format("LogTrackingRaidStartingOn", mapName));
	}

	private static void OnRaidStarted() {
		lock (Gate) {
			_raidActive = true;
		}

		Logger.Notify(Logger.NotifyLevel.Success, LocalizationService.Format("LogTrackingRaidStarted"));
	}

	private static void OnRaidExited() => OnRaidEnded();

	private static void OnRaidEnded() {
		bool wasActive;
		lock (Gate) {
			wasActive = _raidActive;
			_raidActive = false;
		}

		// The game reports this on leaving the post-raid screens too, which would
		// otherwise announce a second end to a raid that already finished.
		if (!wasActive) return;

		Logger.Notify(Logger.NotifyLevel.Success, LocalizationService.Format("LogTrackingRaidEnded"));
	}

	private static void OnMatchingAborted() {
		lock (Gate) {
			_raidActive = false;
		}

		Logger.Notify(Logger.NotifyLevel.Warning, LocalizationService.Format("LogTrackingRaidMatchingAborted"));
	}

	/// <summary>The tracked map's display name, or empty when it is unknown.</summary>
	private static string MapName() {
		var id = CurrentMapId;
		if (string.IsNullOrEmpty(id)) return "";

		return TarkovDevAPI.GetMaps().FirstOrDefault(m => m.Id == id)?.Name ?? "";
	}
}