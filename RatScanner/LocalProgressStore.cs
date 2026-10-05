using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RatScanner.FetchModels.TarkovTracker;
using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RatScanner;

/// <summary>
/// Player progress kept on this machine, used when TarkovTracker is not
/// connected. Stored in the same shape as a TarkovTracker backup export, so the
/// file can be read by that tool and progress moved between the two without a
/// conversion step.
/// </summary>
public class LocalProgressStore {
    private const string Format = "tarkovtracker-backup";
    private const int Version = 2;

    private static readonly string FilePath = Path.Combine(RatConfig.Paths.UserData, "localprogress.json");

    private static readonly object Gate = new();

    /// <summary>Per game mode, objective id to what the player has recorded.</summary>
    private readonly Dictionary<GameMode, Dictionary<string, ObjectiveProgress>> _objectives = new();

    /// <summary>Per game mode, task id to its completion record.</summary>
    private readonly Dictionary<GameMode, Dictionary<string, TaskProgress>> _tasks = new();

    /// <summary>Per game mode, the profile fields the player set by hand.</summary>
    private readonly Dictionary<GameMode, Profile> _profiles = new();

    /// <summary>Raised on any change, and on a game mode change since progress is kept per mode.</summary>
    public static event Action? Changed;

    public static void NotifyChanged() => Changed?.Invoke();

    private sealed class ObjectiveProgress {
        public bool Complete { get; set; }
        public int Count { get; set; }
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// A task's recorded status, in the shape TarkovTracker uses: a failed task
    /// also carries <see cref="Complete"/>, because failure is a completion of
    /// sorts, and the reader gives <see cref="Failed"/> precedence.
    /// </summary>
    private sealed class TaskProgress {
        public bool Complete { get; set; }
        public bool Failed { get; set; }
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// The parts of a profile the tracker does not report anywhere else: the PMC
    /// level, which side the player fights for, and per-trader standing. Null
    /// means "not set", which the quest gates read as "unknown" rather than as a
    /// zero.
    /// </summary>
    private sealed class Profile {
        public int? Level { get; set; }
        public string? Faction { get; set; }
        public Dictionary<string, TraderProgress> Traders { get; set; } = new(StringComparer.Ordinal);
    }

    private sealed class TraderProgress {
        public int? Level { get; set; }
        public int? Reputation { get; set; }
    }

    public LocalProgressStore() {
        Load();
    }

    /// <summary>Whether the player has recorded anything at all.</summary>
    public bool HasProgress {
        get {
            lock (Gate) {
                return _objectives.Any(m => m.Value.Count > 0) || _tasks.Any(m => m.Value.Count > 0);
            }
        }
    }

    /// <summary>
    /// The recorded progress as the same shape TarkovTracker reports, so the quest
    /// views can read either source without caring which one they got. Cached per
    /// game mode.
    /// </summary>
    public UserProgress GetProgress() {
        lock (Gate) {
            if (_cached is not null && _cachedMode == RatConfig.GameMode) return _cached;

            var progress = new UserProgress { UserId = "local", DisplayName = "Local" };

            _profiles.TryGetValue(RatConfig.GameMode, out var profile);
            if (profile is not null) {
                if (profile.Level is int level) progress.PlayerLevel = level;
                if (!string.IsNullOrWhiteSpace(profile.Faction)) progress.PmcFaction = profile.Faction;

                progress.Traders = [.. profile.Traders
                    .Where(t => t.Value.Level is not null || t.Value.Reputation is not null)
                    .Select(t => new UserProgress.TraderProgress {
                        Id = t.Key,
                        Level = t.Value.Level,
                        Reputation = t.Value.Reputation,
                    })];
            }

            if (_objectives.TryGetValue(RatConfig.GameMode, out var objectives)) {
                progress.TaskObjectives = [.. objectives.Select(o => new Progress {
                    Id = o.Key,
                    Complete = o.Value.Complete,
                    Count = o.Value.Count,
                })];
            }

            if (_tasks.TryGetValue(RatConfig.GameMode, out var tasks)) {
                progress.Tasks = [.. tasks.Select(t => new Progress {
                    Id = t.Key,
                    Complete = t.Value.Complete,
                    Failed = t.Value.Failed,
                })];
            }

            _cached = progress;
            _cachedMode = RatConfig.GameMode;
            return progress;
        }
    }

    /// <summary>
    /// Sets the PMC level for the current game mode, or clears it when null so
    /// the quest gates fall back to treating the level as unknown.
    /// </summary>
    public void SetLevel(int? level) {
        lock (Gate) {
            ProfileFor(RatConfig.GameMode).Level = level is int value && value > 0 ? value : null;
            Persist();
        }

        RaiseChanged();
    }

    /// <summary>
    /// Sets which side the player fights for. Null or empty clears it, which the
    /// faction gate reads as unknown rather than as a mismatch.
    /// </summary>
    public void SetFaction(string? faction) {
        lock (Gate) {
            var trimmed = faction?.Trim();
            ProfileFor(RatConfig.GameMode).Faction =
                string.IsNullOrEmpty(trimmed) ? null : trimmed.ToUpperInvariant();
            Persist();
        }

        RaiseChanged();
    }

    /// <summary>
    /// Sets a trader's standing. Either part may be null to leave it unset, and
    /// an entry with nothing left set is dropped so the file does not fill up with
    /// empty records.
    /// </summary>
    public void SetTrader(string traderId, int? level, int? reputation) {
        if (string.IsNullOrWhiteSpace(traderId)) return;

        lock (Gate) {
            var traders = ProfileFor(RatConfig.GameMode).Traders;

            if (level is null && reputation is null) {
                traders.Remove(traderId);
            } else {
                traders[traderId] = new TraderProgress { Level = level, Reputation = reputation };
            }

            Persist();
        }

        RaiseChanged();
    }

    /// <summary>
    /// Drops every manually set profile field for the current game mode, so the
    /// quest gates go back to treating level, faction and standing as unknown.
    /// Recorded objectives and tasks are left alone.
    /// </summary>
    public void ClearProfile() {
        lock (Gate) {
            if (_profiles.Remove(RatConfig.GameMode)) Persist();
        }

        RaiseChanged();
    }

    /// <summary>
    /// What a game mode holds, for the settings screen to report before wiping it.
    ///
    /// Only the objectives and task completions are counted, which is exactly
    /// what a progress reset erases. Profile fields (level, faction, trader
    /// standing) are deliberately left out: a mode can hold those alone, and
    /// clearing quests would not touch them, so counting them would offer the
    /// player a reset that does nothing.
    /// </summary>
    public readonly record struct ProgressCounts(int Objectives, int Tasks) {
        /// <summary>Whether the mode holds recorded objectives or task completions.</summary>
        public bool HasRecorded => Objectives > 0 || Tasks > 0;
    }

    /// <summary>
    /// How much is recorded for a game mode. Used to warn before a reset and to
    /// grey out a mode with nothing to erase.
    /// </summary>
    public ProgressCounts GetCounts(GameMode mode) {
        lock (Gate) {
            var objectives = _objectives.TryGetValue(mode, out var o) ? o.Count : 0;
            var tasks = _tasks.TryGetValue(mode, out var t) ? t.Count : 0;

            return new ProgressCounts(objectives, tasks);
        }
    }

    /// <summary>
    /// Erases the recorded objectives and task completions for the given game
    /// modes, leaving the other modes untouched.
    ///
    /// Deliberately does not touch the profile fields (level, faction, trader
    /// standing). Those are settings the player typed in rather than progress
    /// the game reported, and wiping a level because a quest list was cleared
    /// would quietly change which quests the gates consider available. The
    /// profile has its own reset on the same screen for that.
    /// </summary>
    public void ResetProgress(IEnumerable<GameMode> modes) {
        var removed = false;

        lock (Gate) {
            foreach (var mode in modes) {
                removed |= _objectives.Remove(mode);
                removed |= _tasks.Remove(mode);
            }

            if (removed) Persist();
        }

        if (removed) RaiseChanged();
    }

    private UserProgress? _cached;
    private GameMode? _cachedMode;

    /// <summary>Drops the cached view so the next read reflects an edit.</summary>
    private void Invalidate() => _cached = null;

    /// <summary>Invalidates the cached view and writes the file, both under the lock.</summary>
    private void Persist() {
        Invalidate();
        Save();
    }

    private Profile ProfileFor(GameMode mode) {
        if (!_profiles.TryGetValue(mode, out var profile)) {
            profile = new Profile();
            _profiles[mode] = profile;
        }

        return profile;
    }

    /// <summary>
    /// Sets an objective's completion. Marking it done also fills the counter up
    /// to what the objective asks for, so the two never disagree.
    /// </summary>
    public void SetObjectiveComplete(string objectiveId, bool complete, int required) {
        if (string.IsNullOrEmpty(objectiveId)) return;

        lock (Gate) {
            var objectives = MapFor(_objectives);
            var entry = objectives.GetValueOrDefault(objectiveId);
            if (entry == null) {
                entry = new ObjectiveProgress();
                objectives[objectiveId] = entry;
            }

            entry.Complete = complete;
            entry.Timestamp = Now();

            if (complete && required > 0) entry.Count = Math.Max(entry.Count, required);
            if (!complete && entry.Count >= required && required > 0) entry.Count = 0;

            Persist();
        }

        RaiseChanged();
    }

    /// <summary>
    /// Moves an objective's counter by <paramref name="delta"/>, clamped to the
    /// amount the objective asks for. Reaching that amount completes it, and
    /// dropping back below stops it being complete again.
    /// </summary>
    public void StepObjective(string objectiveId, int delta, int required) {
        if (string.IsNullOrEmpty(objectiveId) || delta == 0) return;

        lock (Gate) {
            var objectives = MapFor(_objectives);
            var entry = objectives.GetValueOrDefault(objectiveId);
            if (entry == null) {
                entry = new ObjectiveProgress();
                objectives[objectiveId] = entry;
            }

            var count = Math.Clamp(entry.Count + delta, 0, Math.Max(required, 0));
            entry.Count = count;
            entry.Complete = required > 0 ? count >= required : entry.Complete;
            entry.Timestamp = Now();

            Persist();
        }

        RaiseChanged();
    }

    /// <summary>Records whether a whole task is done.</summary>
    public void SetTaskComplete(string taskId, bool complete) {
        SetTaskStatus(taskId, complete, false);
    }

    /// <summary>
    /// Records a task's status. A failed task is stored as complete and failed,
    /// matching TarkovTracker, so the two sources read the same.
    /// </summary>
    public void SetTaskFailed(string taskId, bool failed) {
        SetTaskStatus(taskId, failed, failed);
    }

    private void SetTaskStatus(string taskId, bool complete, bool failed) {
        if (string.IsNullOrEmpty(taskId)) return;

        lock (Gate) {
            var tasks = MapFor(_tasks);
            var entry = tasks.GetValueOrDefault(taskId);
            if (entry == null) {
                entry = new TaskProgress();
                tasks[taskId] = entry;
            }

            // Failure wins over completion, so recording a completion clears a
            // failure and recording a failure clears nothing else.
            if (failed) {
                entry.Complete = true;
                entry.Failed = true;
            } else {
                entry.Complete = complete;
                entry.Failed = false;
            }

            entry.Timestamp = Now();
            Persist();
        }

        RaiseChanged();
    }

    private static void RaiseChanged() => NotifyChanged();

    private Dictionary<string, ObjectiveProgress> MapFor(Dictionary<GameMode, Dictionary<string, ObjectiveProgress>> map) =>
        MapFor(map, RatConfig.GameMode);

    private Dictionary<string, ObjectiveProgress> MapFor(Dictionary<GameMode, Dictionary<string, ObjectiveProgress>> map, GameMode mode) {
        if (!map.TryGetValue(mode, out var value)) {
            value = [];
            map[mode] = value;
        }

        return value;
    }

    private Dictionary<string, TaskProgress> MapFor(Dictionary<GameMode, Dictionary<string, TaskProgress>> map) =>
        MapFor(map, RatConfig.GameMode);

    private Dictionary<string, TaskProgress> MapFor(Dictionary<GameMode, Dictionary<string, TaskProgress>> map, GameMode mode) {
        if (!map.TryGetValue(mode, out var value)) {
            value = [];
            map[mode] = value;
        }

        return value;
    }

    private static long Now() => DateTimeOffset.Now.ToUnixTimeMilliseconds();

    private void Load() {
        try {
            if (!File.Exists(FilePath)) return;

            var root = JObject.Parse(File.ReadAllText(FilePath));

            foreach (var (mode, section) in Modes()) {
                if (root[section] is not JObject data) continue;

                var profile = ProfileFor(mode);

                if (data["level"] is JValue levelNode && (int?)levelNode > 0) {
                    profile.Level = (int?)levelNode;
                }

                if (data["pmcFaction"] is JValue factionNode && factionNode.Type != JTokenType.Null) {
                    profile.Faction = factionNode.Value<string>();
                }

                if (data["traders"] is JObject traderNode) {
                    foreach (var (id, node) in traderNode) {
                        profile.Traders[id] = new TraderProgress {
                            Level = (int?)node["level"],
                            Reputation = (int?)node["reputation"],
                        };
                    }
                }

                if (data["taskObjectives"] is JObject objectiveNode) {
                    var objectives = MapFor(_objectives, mode);
                    foreach (var (id, node) in objectiveNode) {
                        objectives[id] = new ObjectiveProgress {
                            Complete = (bool?)node["complete"] ?? false,
                            Count = (int?)node["count"] ?? 0,
                            Timestamp = (long?)node["timestamp"] ?? 0,
                        };
                    }
                }

                if (data["taskCompletions"] is JObject taskNode) {
                    var tasks = MapFor(_tasks, mode);
                    foreach (var (id, node) in taskNode) {
                        tasks[id] = new TaskProgress {
                            Complete = (bool?)node["complete"] ?? false,
                            Failed = (bool?)node["failed"] ?? false,
                            Timestamp = (long?)node["timestamp"] ?? 0,
                        };
                    }
                }
            }
        } catch (Exception e) {
            Logger.LogWarning($"Could not read local progress, starting empty: {e.Message}");
        }
    }

    private void Save() {
        try {
            var root = new JObject {
                ["_format"] = Format,
                ["_version"] = Version,
                ["exportedAt"] = Now(),
                ["currentGameMode"] = SectionFor(RatConfig.GameMode),
            };

            foreach (var (mode, section) in Modes()) {
                var data = new JObject();

                // The profile fields are written on every save, unset ones as null,
                // so the file stays a valid TarkovTracker backup that imports
                // cleanly rather than one whose sections are missing keys.
                _profiles.TryGetValue(mode, out var profile);
                data["level"] = profile?.Level;
                data["pmcFaction"] = profile?.Faction;
                data["xpOffset"] = 0;

                if (profile is not null && profile.Traders.Count > 0) {
                    var traders = new JObject();
                    foreach (var (id, trader) in profile.Traders) {
                        traders[id] = new JObject {
                            ["level"] = trader.Level,
                            ["reputation"] = trader.Reputation,
                        };
                    }

                    data["traders"] = traders;
                } else {
                    data["traders"] = new JObject();
                }

                if (_objectives.TryGetValue(mode, out var objectives) && objectives.Count > 0) {
                    var node = new JObject();
                    foreach (var (id, entry) in objectives) {
                        node[id] = new JObject {
                            ["complete"] = entry.Complete,
                            ["count"] = entry.Count,
                        };

                        if (entry.Timestamp > 0) node[id]["timestamp"] = entry.Timestamp;
                    }

                    data["taskObjectives"] = node;
                } else {
                    data["taskObjectives"] = new JObject();
                }

                if (_tasks.TryGetValue(mode, out var tasks) && tasks.Count > 0) {
                    var node = new JObject();
                    foreach (var (id, entry) in tasks) {
                        var record = new JObject {
                            ["complete"] = entry.Complete,
                            ["failed"] = entry.Failed,
                        };

                        if (entry.Timestamp > 0) record["timestamp"] = entry.Timestamp;
                        node[id] = record;
                    }

                    data["taskCompletions"] = node;
                } else {
                    data["taskCompletions"] = new JObject();
                }

                root[section] = data;
            }

            _ = Directory.CreateDirectory(RatConfig.Paths.UserData);
            File.WriteAllText(FilePath, root.ToString(Formatting.Indented));
        } catch (Exception e) {
            Logger.LogWarning($"Could not save local progress: {e.Message}");
        }
    }

    /// <summary>Game mode to backup section name, matching a TarkovTracker export.</summary>
    private static IEnumerable<(GameMode Mode, string Section)> Modes() {
        yield return (GameMode.Regular, "pvp");
        yield return (GameMode.Pve, "pve");
        yield return (GameMode.PvpSeason, "seasonal");
    }

    private static string SectionFor(GameMode mode) => Modes().First(m => m.Mode == mode).Section;
}