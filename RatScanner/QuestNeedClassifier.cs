using RatScanner.FetchModels.TarkovTracker;
using RatScanner.TarkovDev;
using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static RatScanner.TarkovDev.Json.Item;

namespace RatScanner;

/// <summary>
/// Where a task's item requirement stands relative to the player's actual progress.
/// Conservative by design: conditions RatScanner cannot evaluate preserve uncertainty.
/// </summary>
internal enum QuestGate {
	/// <summary>Task is complete/failed (or its progress entry was invalidated) — never counts.</summary>
	NotApplicable,

	/// <summary>All modeled gates are open; active vs available is decided with progress context.</summary>
	ApplicableNow,

	/// <summary>Gates open and the task shows progress — the requirement applies right now.</summary>
	ActiveNow,

	/// <summary>Gates open but the task was not started — it can be accepted/progressed now.</summary>
	AvailableNow,

	/// <summary>Locked behind a known gate (player level and/or prerequisite task).</summary>
	FutureKnown,

	/// <summary>
	/// Gated by a condition RatScanner cannot evaluate (trader reputation/loyalty,
	/// faction when unknown, dialogue/timed/Lightkeeper unlocks).
	/// </summary>
	ConditionalUnknown,
}

/// <summary>Item needs for one scanned item, split by applicability bucket.</summary>
internal sealed record QuestNeedReport {
	public int ActiveNow { get; init; }
	public int AvailableNow { get; init; }
	public int FutureKnown { get; init; }
	public int ConditionalUnknown { get; init; }

	public int ActiveNowFir { get; init; }
	public int AvailableNowFir { get; init; }
	public int FutureKnownFir { get; init; }
	public int ConditionalUnknownFir { get; init; }

	/// <summary>Remaining giveItem/buildWeapon weapon needs per bucket (assembly unverifiable).</summary>
	public int ActiveWeaponHandIn { get; init; }
	public int AvailableWeaponHandIn { get; init; }
	public int FutureWeaponHandIn { get; init; }
	public int ConditionalWeaponHandIn { get; init; }

	/// <summary>Earliest known player level that unlocks a FutureKnown need, when known.</summary>
	public int? UnlockLevel { get; init; }

	/// <summary>Remaining needs in incomplete kappa-required tasks across all buckets.</summary>
	public int KappaTotal { get; init; }

	public int CurrentTotal => ActiveNow + AvailableNow;
	public int GrandTotal => ActiveNow + AvailableNow + FutureKnown + ConditionalUnknown;
	public bool Any => GrandTotal > 0;
	public int FirTotal => ActiveNowFir + AvailableNowFir + FutureKnownFir + ConditionalUnknownFir;
	public int WeaponHandInTotal =>
		ActiveWeaponHandIn + AvailableWeaponHandIn + FutureWeaponHandIn + ConditionalWeaponHandIn;
}

internal readonly record struct ObjectiveNeedBreakdown(
	int Total,
	int FoundInRaid,
	int NonFoundInRaid,
	int WeaponHandIn
);

/// <summary>
/// Classifies quest item requirements using only data sources RatScanner actually has:
/// entity progress (tasks + objectives + player level + faction from TarkovTracker) and
/// json.tarkov.dev task gates (prerequisite tasks, player level, faction, trader gates).
/// Trader standing/scav karma is NOT exposed by the tracker API, so reputation-gated
/// tasks (e.g. the Fence "Compensation for Damage" series) stay conditional instead of
/// inflating "needed now" counts.
/// </summary>
internal static class QuestNeedClassifier {
	private const string StatusActive = "active";
	private const string StatusComplete = "complete";
	private const string StatusFailed = "failed";

	// Collector-style event tasks are intentionally excluded from item needs.
	// Keep the single list here so current, future, conditional, and kappa totals
	// cannot drift apart.
	private static readonly HashSet<string> ExcludedTaskIds = new(StringComparer.Ordinal)
	{
		"61e6e5e0f5b9633f6719ed95",
		"61e6e60223374d168a4576a6",
		"61e6e621bfeab00251576265",
		"61e6e615eea2935bc018a2c5",
		"61e6e60c5ca3b3783662be27",
	};

	public static QuestNeedReport Classify(Item item, IReadOnlyList<TarkovTask> tasks, UserProgress progress, bool showNonFir) {
		ArgumentNullException.ThrowIfNull(item);
		ArgumentNullException.ThrowIfNull(progress);

		Dictionary<string, TarkovTask> tasksById = TarkovDevAPI.GetTasks().ToDictionary(t => t.Id, t => t);

		int active = 0,
			available = 0,
			future = 0,
			conditional = 0;
		int activeFir = 0,
			availableFir = 0,
			futureFir = 0,
			conditionalFir = 0;
		int activeWeapon = 0,
			availableWeapon = 0,
			futureWeapon = 0,
			conditionalWeapon = 0;
		int? unlockLevel = null;
		int kappa = 0;

		foreach (TarkovTask task in tasks) {
			if (ExcludedTaskIds.Contains(task.Id))
				continue;

			// Recorded, non-invalid objective progress proves the player already
			// started this task. Unlock gates (trader standing, delays,
			// Lightkeeper dialogue, active-only prerequisites) have therefore
			// already been crossed and must not demote a live need to uncertain.
			bool taskShowsProgress = TaskShowsProgress(task, progress);
			QuestGate gate = ClassifyGate(task, tasksById, progress, out int? taskUnlockLevel);
			if (gate != QuestGate.NotApplicable && taskShowsProgress)
				gate = QuestGate.ActiveNow;
			if (gate == QuestGate.NotApplicable)
				continue;

			ObjectiveNeedBreakdown need = GetObjectiveNeedBreakdown(
				item,
				task.Objectives,
				progress,
				showNonFir
			);
			if (need.Total <= 0)
				continue;

			if (task.KappaRequired == true)
				kappa += need.Total;

			if (gate == QuestGate.ApplicableNow)
				gate = QuestGate.AvailableNow;

			switch (gate) {
				case QuestGate.ActiveNow:
					active += need.Total;
					activeFir += need.FoundInRaid;
					activeWeapon += need.WeaponHandIn;
					break;
				case QuestGate.AvailableNow:
					available += need.Total;
					availableFir += need.FoundInRaid;
					availableWeapon += need.WeaponHandIn;
					break;
				case QuestGate.FutureKnown:
					future += need.Total;
					futureFir += need.FoundInRaid;
					futureWeapon += need.WeaponHandIn;
					if (taskUnlockLevel is int level)
						unlockLevel = unlockLevel is null ? level : Math.Min(unlockLevel.Value, level);
					break;
				case QuestGate.ConditionalUnknown:
					conditional += need.Total;
					conditionalFir += need.FoundInRaid;
					conditionalWeapon += need.WeaponHandIn;
					break;
			}
		}

		return new QuestNeedReport {
			ActiveNow = active,
			AvailableNow = available,
			FutureKnown = future,
			ConditionalUnknown = conditional,
			ActiveNowFir = activeFir,
			AvailableNowFir = availableFir,
			FutureKnownFir = futureFir,
			ConditionalUnknownFir = conditionalFir,
			ActiveWeaponHandIn = activeWeapon,
			AvailableWeaponHandIn = availableWeapon,
			FutureWeaponHandIn = futureWeapon,
			ConditionalWeaponHandIn = conditionalWeapon,
			UnlockLevel = unlockLevel,
			KappaTotal = kappa,
		};
	}

	/// <summary>Gate evaluation without item context.</summary>
	internal static QuestGate ClassifyGate(
		TarkovTask task,
		IReadOnlyDictionary<string, TarkovTask> tasksById,
		UserProgress progress,
		out int? unlockLevel
	) {
		unlockLevel = null;

		Progress? entry = progress.Tasks.FirstOrDefault(p => p.Id == task.Id);
		if (entry is { Complete: true } or { Failed: true } or { Invalid: true })
			return QuestGate.NotApplicable;

		bool conditional = false;
		bool future = false;

		// Faction gate: evaluable when the tracker profile carries a faction.
		if (
			!string.IsNullOrEmpty(task.FactionName)
			&& !task.FactionName.Equals("Any", StringComparison.OrdinalIgnoreCase)
		) {
			if (string.IsNullOrEmpty(progress.PmcFaction))
				conditional = true;
			else if (!task.FactionName.Equals(progress.PmcFaction, StringComparison.OrdinalIgnoreCase))
				return QuestGate.NotApplicable;
		}

		if (task.TaskRequirements is { Count: > 0 }) {
			foreach (TaskRequirement prerequisite in task.TaskRequirements) {
				switch (PrerequisiteStatus(prerequisite, tasksById, progress)) {
					case PrerequisiteGate.Satisfied:
						continue;
					case PrerequisiteGate.MismatchedEndState:
						// The prerequisite ended (complete/failed) in a state the
						// requirement does not accept — this task can never unlock.
						return QuestGate.NotApplicable;
					case PrerequisiteGate.LockedByKnownGate:
						future = true;
						continue;
					default:
						// "active"-only prerequisites we cannot verify keep uncertainty.
						conditional = true;
						continue;
				}
			}
		}

		if (task.MinPlayerLevel > 1) {
			if (progress.PlayerLevel is int level) {
				if (level < task.MinPlayerLevel) {
					future = true;
					unlockLevel = task.MinPlayerLevel;
				}
			} else {
				conditional = true;
			}
		}

		// Trader standing / loyalty gates: not exposed by the tracker API.
		if (task.TraderRequirements is { Count: > 0 })
			conditional = true;

		if (task.HasUnmodeledRequirements)
			conditional = true;

		if (conditional)
			return QuestGate.ConditionalUnknown;
		if (future)
			return QuestGate.FutureKnown;
		return QuestGate.ApplicableNow;
	}

	private enum PrerequisiteGate {
		Satisfied,

		// Neither complete nor failed, and the requirement accepts an end state
		// → the task is locked behind a known, reachable prerequisite.
		LockedByKnownGate,

		// Neither complete nor failed, but the requirement only accepts an in-progress
		// ("active") prerequisite → not verifiable from tracker data.
		UnverifiableActiveState,

		// The prerequisite already ended in a state the requirement does not accept
		// → the task can never become available.
		MismatchedEndState,
	}

	private static PrerequisiteGate PrerequisiteStatus(
		TaskRequirement requirement,
		IReadOnlyDictionary<string, TarkovTask> tasksById,
		UserProgress progress
	) {
		Progress? entry = progress.Tasks.FirstOrDefault(p => p.Id == requirement.Task);
		bool complete = entry?.Complete == true;
		bool failed = entry?.Failed == true;

		HashSet<string> statuses = new(requirement.Status, StringComparer.OrdinalIgnoreCase);
		bool wantsActive = statuses.Contains(StatusActive);

		if (complete) {
			if (statuses.Contains(StatusComplete))
				return PrerequisiteGate.Satisfied;
			// An active-only requirement may have unlocked the dependent task
			// before it completed. Without progress on the dependent task this is
			// uncertain, not proof that the dependent task is permanently dead.
			return wantsActive ? PrerequisiteGate.UnverifiableActiveState : PrerequisiteGate.MismatchedEndState;
		}
		if (failed)
			return statuses.Contains(StatusFailed) ? PrerequisiteGate.Satisfied : PrerequisiteGate.MismatchedEndState;
		// Prereq neither complete nor failed: satisfied only when the requirement
		// accepts an in-progress ("active") task AND the tracker shows progress on it.
		if (wantsActive && !statuses.Contains(StatusComplete) && !statuses.Contains(StatusFailed)) {
			if (
				tasksById.TryGetValue(requirement.Task, out TarkovTask? prereqTask)
				&& TaskShowsProgress(prereqTask, progress)
			)
				return PrerequisiteGate.Satisfied;
			return PrerequisiteGate.UnverifiableActiveState;
		}

		return PrerequisiteGate.LockedByKnownGate;
	}

	/// <summary>
	/// The tracker only records completed/failed tasks; started-but-unfinished tasks
	/// are observable through objective progress entries.
	/// </summary>
	private static bool TaskShowsProgress(TarkovTask task, UserProgress progress) {
		Progress? taskEntry = progress.Tasks.FirstOrDefault(p => p.Id == task.Id);
		if (taskEntry is { Complete: true } or { Failed: true } or { Invalid: true })
			return false;

		return task.Objectives is { Count: > 0 }
			&& task.Objectives.Any(o =>
				o.Id is not null && progress.TaskObjectives.Any(p => p.Id == o.Id && !p.Invalid)
			);
	}
}
