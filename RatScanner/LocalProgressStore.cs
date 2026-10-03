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

    private UserProgress? _cached;
    private GameMode? _cachedMode;

    /// <summary>Drops the cached view so the next read reflects an edit.</summary>
    private void Invalidate() => _cached = null;

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

            Invalidate();
            Save();
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

            Invalidate();
            Save();
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
            Invalidate();
            Save();
        }

        RaiseChanged();
    }

    private static void RaiseChanged() => NotifyChanged();

    private Dictionary<string, ObjectiveProgress> MapFor(Dictionary<GameMode, Dictionary<string, ObjectiveProgress>> map) {
        if (!map.TryGetValue(RatConfig.GameMode, out var value)) {
            value = [];
            map[RatConfig.GameMode] = value;
        }

        return value;
    }

    private Dictionary<string, TaskProgress> MapFor(Dictionary<GameMode, Dictionary<string, TaskProgress>> map) {
        if (!map.TryGetValue(RatConfig.GameMode, out var value)) {
            value = [];
            map[RatConfig.GameMode] = value;
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

                if (data["taskObjectives"] is JObject objectiveNode) {
                    var objectives = MapFor(_objectives);
                    foreach (var (id, node) in objectiveNode) {
                        objectives[id] = new ObjectiveProgress {
                            Complete = (bool?)node["complete"] ?? false,
                            Count = (int?)node["count"] ?? 0,
                            Timestamp = (long?)node["timestamp"] ?? 0,
                        };
                    }
                }

                if (data["taskCompletions"] is JObject taskNode) {
                    var tasks = MapFor(_tasks);
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