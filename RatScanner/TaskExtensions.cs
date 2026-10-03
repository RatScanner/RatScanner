using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RatScanner;

/// <summary>
/// Presentation helpers for quests: task graph lookups, tracker state and the
/// reward data the task detail view renders.
/// </summary>
public static class TaskExtensions {
    /// <summary>
    /// Objectives of the task with their TarkovTracker completion state and counter.
    /// </summary>
    public static List<QuestObjectiveEntry> GetObjectives(this TarkovTask task) =>
        Item.GetObjectiveProgress(task, Item.SelfProgress);

    /// <summary>
    /// Prerequisites the task lists, with the state each one is in according to
    /// TarkovTracker. Requirements the player already satisfies are reported by
    /// tarkov.dev as "complete", but tracker progress is authoritative here.
    /// </summary>
    public static List<TaskRequirementEntry> GetPrerequisites(this TarkovTask task) {
        var progress = Item.SelfProgress;
        var result = new List<TaskRequirementEntry>();

        foreach (var requirement in task.TaskRequirements ?? []) {
            var prereq = TarkovDevAPI.GetTasks().FirstOrDefault(t => t.Id == requirement.Task);
            if (prereq is null) {
                continue;
            }

            var isComplete = Item.IsTaskComplete(prereq, progress);
            var isFailed = Item.IsTaskFailed(prereq, progress);
            var status = GetStatus(requirement.Status, isComplete, isFailed, task, prereq);

            result.Add(new TaskRequirementEntry(prereq, status, isComplete, isFailed));
        }

        return result;
    }

    /// <summary>
    /// Tasks this one is a prerequisite of, i.e. the quests it leads into.
    /// </summary>
    public static List<TarkovTask> GetUnlocks(this TarkovTask task) {
        if (task.Id is null) {
            return [];
        }

        return [.. TarkovDevAPI.GetTasks().Where(t =>
            t.Id != task.Id
            && t.TaskRequirements != null
            && t.TaskRequirements.Any(r => r.Task == task.Id)
        )];
    }

    /// <summary>
    /// How many of the task's prerequisites are done, for the progress chip.
    /// </summary>
    public static (int complete, int total) GetPrerequisiteProgress(this TarkovTask task) {
        var prerequisites = GetPrerequisites(task);
        return (prerequisites.Count(p => p.IsComplete), prerequisites.Count);
    }

    /// <summary>
    /// How many of the task's objectives are done according to the tracker.
    /// Objectives that are complete because the whole task is complete count as
    /// done, matching what the item quest tables show.
    /// </summary>
    public static (int complete, int total) GetObjectiveProgressSummary(this TarkovTask task) {
        var objectives = GetObjectives(task);
        if (objectives.Count == 0) {
            return (0, 0);
        }

        var taskComplete = Item.IsTaskComplete(task, Item.SelfProgress);
        return (objectives.Count(o => taskComplete || o.IsComplete), objectives.Count);
    }
    /// <summary>
    /// Whether the task can be taken on right now: no failed prerequisite, all
    /// prerequisites done, and the player level requirement met.
    /// </summary>
    public static bool IsUnlocked(this TarkovTask task) {
        var prerequisites = GetPrerequisites(task);
        if (prerequisites.Any(p => p.IsFailed)) {
            return false;
        }

        if (prerequisites.Count > 0 && prerequisites.Any(p => !p.IsComplete)) {
            return false;
        }

        return task.MinPlayerLevel <= PlayerLevel;
    }

    /// <summary>Player level from the tracker, or 0 when it is unknown.</summary>
    private static int PlayerLevel => Item.SelfProgress.PlayerLevel ?? 0;
    private static TaskStatus GetStatus(List<string>? reported, bool isComplete, bool isFailed, TarkovTask task,
        TarkovTask prereq) {
        if (isComplete) {
            return TaskStatus.Complete;
        }

        if (isFailed) {
            return TaskStatus.Failed;
        }

        var status = reported?.FirstOrDefault()?.ToLowerInvariant() switch {
            "complete" => TaskStatus.Complete,
            "active" => TaskStatus.Active,
            "ready" => TaskStatus.Ready,
            "possible" => TaskStatus.Possible,
            "failed" => TaskStatus.Failed,
            _ => TaskStatus.Unknown,
        };

        return status == TaskStatus.Complete ? TaskStatus.Active : status;
    }

    /// <summary>
    /// Trader level at which the giving trader starts handing out the task, or
    /// null when there is no such gate. This is the trader's own loyalty level,
    /// distinct from <see cref="TarkovTask.MinPlayerLevel"/>, which is the player
    /// level the task needs.
    /// </summary>
    public static int? GetTraderOfferLevel(this TarkovTask task) {
        var traderId = task.TraderId;
        if (string.IsNullOrEmpty(traderId)) {
            return null;
        }

        var requirement = task.TraderRequirements?.FirstOrDefault(r =>
            string.Equals(r.TraderId, traderId, StringComparison.Ordinal));

        // Only a loyalty level gates when the task becomes offered; the other
        // requirement types are not a level.
        if (requirement is null
            || !string.Equals(requirement.RequirementType, "loyaltyLevel", StringComparison.OrdinalIgnoreCase)) {
            return null;
        }

        return requirement.Value > 0 ? requirement.Value : null;
    }

    /// <summary>
    /// Items handed out when the task is completed, resolved against the item catalogue.
    /// </summary>
    public static List<TaskRewardEntry> GetFinishRewards(this TarkovTask task) {
        var result = new List<TaskRewardEntry>();
        var rewards = task.FinishRewards?.Items;
        if (rewards is not { Count: > 0 }) {
            return result;
        }

        var lookup = GetItemLookup();
        foreach (var reward in rewards) {
            lookup.TryGetValue(reward.Item, out var item);
            result.Add(new TaskRewardEntry(reward.Count, item));
        }

        return result;
    }

    /// <summary>
    /// Trader standing gained by completing the task.
    /// </summary>
    public static List<TaskStandingEntry> GetTraderStandingRewards(this TarkovTask task) {
        var result = new List<TaskStandingEntry>();
        var rewards = task.FinishRewards?.TraderStanding;
        if (rewards is not { Count: > 0 }) {
            return result;
        }

        foreach (var reward in rewards) {
            // TaskTraderStanding also exposes a resolved Trader object, which hides
            // the id this record is keyed on.
            result.Add(new TaskStandingEntry(reward.TraderId, reward.Standing));
        }

        return result;
    }

    private static Dictionary<string, Item>? _itemLookup;

    internal static void InvalidateItemLookup() => _itemLookup = null;

    /// <summary>
    /// Item catalogue keyed by id. Cached because reward lookups would otherwise
    /// be a linear scan over every item on each repaint.
    /// </summary>
    internal static IReadOnlyDictionary<string, Item> GetItemLookup() =>
        _itemLookup ??= TarkovDevAPI.GetItems().ToDictionary(i => i.Id, StringComparer.Ordinal);
}

/// <summary>State of a prerequisite task.</summary>
public enum TaskStatus {
    Unknown,
    Active,
    Complete,
    Ready,
    Possible,
    Failed,
}

/// <summary>A prerequisite task and where the player stands with it.</summary>
public record TaskRequirementEntry(TarkovTask Task, TaskStatus Status, bool IsComplete, bool IsFailed);

/// <summary>An item handed out on task completion.</summary>
public record TaskRewardEntry(int Count, Item? Item);

/// <summary>Trader standing gained on task completion.</summary>
public record TaskStandingEntry(string? TraderId, double Standing) {
    private static Trader[]? _traders;

    /// <summary>Trader this standing belongs to, resolved against the trader list.</summary>
    public Trader? Trader {
        get {
            if (TraderId is null) {
                return null;
            }

            _traders ??= TarkovDevAPI.GetTraders();
            return _traders.FirstOrDefault(t => t.Id == TraderId);
        }
    }

    /// <summary>Standing gain, kept fractional only for the small values traders use.</summary>
    public string StandingText => Standing > 0 && Standing < 1
        ? Standing.ToString("0.00", CultureInfo.InvariantCulture)
        : Math.Round(Standing).ToString(CultureInfo.InvariantCulture);
}