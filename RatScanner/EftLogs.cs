using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RatScanner;

/// <summary>
/// Finds the folders Escape from Tarkov writes its logs and screenshots to.
/// </summary>
/// <remarks>
/// Every method here returns an empty string instead of throwing. A missing game,
/// an unregistered install or a denied registry key is a normal state that just
/// means log tracking has nothing to read.
/// </remarks>
internal static class EftLogs {
    private const string SteamEftAppId = "3932890";

    private static readonly string[] UninstallRegistryPaths = [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov",
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 3932890",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 3932890",
    ];

    /// <summary>
    /// The configured override when set and it exists, otherwise the detected logs
    /// folder. Empty when neither is available.
    /// </summary>
    internal static string LogsFolder {
        get {
            var manual = RatConfig.LogTracking.LogsPathOverride.Trim();
            if (!string.IsNullOrEmpty(manual)) {
                return Directory.Exists(manual) ? Path.GetFullPath(manual) : "";
            }

            return DetectLogsFolder();
        }
    }

    /// <summary>
    /// The folder the game drops screenshots into. Only exists once the player has
    /// taken one, so an empty result is expected on a fresh install.
    /// </summary>
    internal static string ScreenshotsFolder {
        get {
            try {
                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (string.IsNullOrEmpty(documents)) return "";

                // The game puts it under a folder spelled with spaces.
                var path = Path.Combine(documents, "Escape From Tarkov", "Screenshots");
                return Directory.Exists(path) ? path : "";
            } catch (Exception e) {
                Logger.LogWarning($"Could not resolve the EFT screenshots folder: {e.Message}");
                return "";
            }
        }
    }

    private static string DetectLogsFolder() {
        foreach (var installPath in RegistryInstallLocations()) {
            var logs = LogsUnder(installPath);
            if (logs != null) return logs;
        }

        foreach (var libraryPath in SteamLibraries()) {
            var logs = LogsUnder(SteamEftInstallPath(libraryPath));
            if (logs != null) return logs;
        }

        return "";
    }

    /// <summary>The logs folder under an install path, or null when there is none.</summary>
    private static string? LogsUnder(string? installPath) {
        if (string.IsNullOrWhiteSpace(installPath)) return null;

        foreach (var relative in new[] { "Logs", Path.Combine("build", "Logs") }) {
            try {
                var path = Path.Combine(installPath, relative);
                if (Directory.Exists(path)) return Path.GetFullPath(path);
            } catch (Exception e) {
				Logger.LogWarning($"Could not check a candidate EFT logs folder under {installPath}: {e.Message}");
			}
		}

		return null;
	}

	private static IEnumerable<string> RegistryInstallLocations() {
		var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine }) {
            foreach (var view in new[] { RegistryView.Default, RegistryView.Registry32, RegistryView.Registry64 }) {
                try {
                    using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                    foreach (var registryPath in UninstallRegistryPaths) {
                        using var key = baseKey.OpenSubKey(registryPath);
                        var location = key?.GetValue("InstallLocation")?.ToString();
                        if (!string.IsNullOrWhiteSpace(location)) found.Add(location);
                    }
                } catch (Exception) {
                    // A denied or missing key just means this view has nothing.
                }
            }
        }

        return found;
    }

    private static IEnumerable<string> SteamLibraries() {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine }) {
            foreach (var view in new[] { RegistryView.Default, RegistryView.Registry32, RegistryView.Registry64 }) {
                try {
                    using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                    using var steamKey = baseKey.OpenSubKey(@"SOFTWARE\Valve\Steam");
                    foreach (var valueName in new[] { "SteamPath", "InstallPath" }) {
                        var steamPath = steamKey?.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrWhiteSpace(steamPath) && Directory.Exists(steamPath)) {
                            roots.Add(Path.GetFullPath(steamPath));
                        }
                    }
                } catch (Exception) {
                }
            }
        }

        var libraries = new HashSet<string>(roots, StringComparer.OrdinalIgnoreCase);

        foreach (var root in roots) {
            var manifest = Path.Combine(root, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(manifest)) continue;

            try {
                var contents = File.ReadAllText(manifest);
                foreach (Match match in Regex.Matches(contents, @"""path""\s+""(?<path>[^""]+)""", RegexOptions.IgnoreCase)) {
                    var libraryPath = match.Groups["path"].Value.Replace("\\\\", "\\");
                    if (Directory.Exists(libraryPath)) libraries.Add(Path.GetFullPath(libraryPath));
                }
            } catch (Exception e) {
                Logger.LogWarning($"Could not read Steam library list: {e.Message}");
            }
        }

        return libraries;
    }

    /// <summary>The EFT install directory inside a Steam library, or null.</summary>
    private static string? SteamEftInstallPath(string libraryPath) {
        var candidates = new List<string>();

        var manifest = Path.Combine(libraryPath, "steamapps", $"appmanifest_{SteamEftAppId}.acf");
        if (File.Exists(manifest)) {
            try {
                var contents = File.ReadAllText(manifest);
                var match = Regex.Match(contents, @"""installdir""\s+""(?<directory>[^""]+)""", RegexOptions.IgnoreCase);
                if (match.Success) {
                    candidates.Add(Path.Combine(libraryPath, "steamapps", "common", match.Groups["directory"].Value));
                }
            } catch (Exception e) {
                Logger.LogWarning($"Could not read Steam app manifest: {e.Message}");
            }
        }

        candidates.Add(Path.Combine(libraryPath, "steamapps", "common", "Escape from Tarkov"));

        foreach (var candidate in candidates) {
            if (Directory.Exists(candidate)) return candidate;
        }

        return null;
    }
}