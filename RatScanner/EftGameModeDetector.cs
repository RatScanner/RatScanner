using RatScanner.TarkovDev.Json;
using System;
using System.Text.RegularExpressions;

namespace RatScanner;

/// <summary>
/// Watches the game's log for the session mode and notices when it differs from
/// the mode RatScanner is set to. Nothing is switched automatically; the user is
/// asked first.
/// </summary>
internal static class EftGameModeDetector {

	// The game has used several spellings for this line across versions, so all of
	// them are accepted rather than only the current one.
	private static readonly Regex SessionModeRegex = new(@"Session mode: (?<mode>[^\s|]+)", RegexOptions.IgnoreCase);

	private static readonly Regex ProfileRegex = new(
		@"(?:Select(?:ed)?Profile|PrepareSelectedProfileLocally) ProfileId:(?<profileId>\w+) AccountId:(?<accountId>\d+)");

	private static readonly object Gate = new();
	private static GameMode? _detected;

	/// <summary>
	/// Raised when the log reports a mode other than the one in use, so the UI can
	/// offer to switch. Carries the mode that was detected.
	/// </summary>
	internal static event Action<GameMode>? ModeDetected;

	/// <summary>
	/// The mode the log last reported, or null when it has not reported a mode we
	/// recognise.
	/// </summary>
	internal static GameMode? DetectedMode {
		get {
			lock (Gate) {
				return _detected;
			}
		}
	}

	/// <summary>The account and profile the log last reported.</summary>
	internal static string ProfileId { get; private set; } = "";

	internal static string AccountId { get; private set; } = "";

	/// <summary>
	/// Feeds a chunk of the application log in. Safe to call with anything; text
	/// that does not parse is ignored.
	/// </summary>
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

				var session = SessionModeRegex.Match(message);
				if (session.Success) {
					ApplySessionMode(session.Groups["mode"].Value);
					continue;
				}

				var profile = ProfileRegex.Match(message);
				if (profile.Success) {
					ProfileId = profile.Groups["profileId"].Value;
					AccountId = profile.Groups["accountId"].Value;
				}
			} catch (Exception e) {
				Logger.LogWarning($"Skipped an unreadable EFT log line: {e.Message}");
			}
		}
	}

	private static void ApplySessionMode(string raw) {
		if (Resolve(raw) is not { } mode) {
			Logger.LogWarning($"EFT reported an unrecognised session mode '{raw}'.");
			return;
		}

		GameMode? previous;
		lock (Gate) {
			previous = _detected;
			_detected = mode;
		}

		if (previous == mode) return;

		Logger.LogInfo($"EFT session mode: {mode}");
		PromptIfDifferent(mode);
	}

	/// <summary>
	/// Asks the user to switch when the detected mode is not the one in use. Only
	/// called on a real change, so a mode already in use never prompts.
	/// </summary>
	private static void PromptIfDifferent(GameMode detected) {
		if (!RatConfig.LogTracking.TrackGameMode) return;
		if (detected == RatConfig.GameMode) return;

		var label = detected switch {
			GameMode.Pve => "PvE",
			GameMode.PvpSeason => "Seasonal",
			_ => "PvP",
		};

		Logger.Notify(Logger.NotifyLevel.Info,
			LocalizationService.Format("LogTrackingModeDetected", label));

		ModeDetected?.Invoke(detected);
	}

	/// <summary>
	/// Maps the game's spelling of the session mode onto RatScanner's. Null for
	/// anything unrecognised so a new mode is ignored rather than guessed at.
	/// </summary>
	internal static GameMode? Resolve(string? raw) {
		if (string.IsNullOrWhiteSpace(raw)) return null;

		if (raw.Equals("Pve", StringComparison.OrdinalIgnoreCase)) return GameMode.Pve;
		if (raw.Equals("Regular", StringComparison.OrdinalIgnoreCase)) return GameMode.Regular;
		if (raw.Equals("PVP", StringComparison.OrdinalIgnoreCase)) return GameMode.Regular;

		if (raw.Equals("PvpSeason", StringComparison.OrdinalIgnoreCase)
			|| raw.Equals("Seasonal", StringComparison.OrdinalIgnoreCase)
			|| raw.Equals("SZN", StringComparison.OrdinalIgnoreCase)) {
			return GameMode.PvpSeason;
		}

		return null;
	}
}