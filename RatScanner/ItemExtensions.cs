using RatScanner.FetchModels.TarkovTracker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RatScanner.TarkovDev.Json;

/// <summary>
/// One quest row relating an item to a task: the amount involved, why, and the
/// TarkovTracker state folded in.
/// </summary>
/// <param name="Task">The quest itself.</param>
/// <param name="Count">Amount required or rewarded.</param>
/// <param name="HasAlternates">Task accepts other items as well.</param>
/// <param name="IsReward">True when the item is given by the task, not kept.</param>
/// <param name="IsComplete">TarkovTracker reports the task as done.</param>
/// <param name="IsFailed">TarkovTracker reports the task as failed.</param>
/// <param name="Objectives">Objectives with their tracker state, for the expanded row.</param>
public record QuestItemEntry(
	TarkovTask Task,
	int Count,
	bool HasAlternates = false,
	bool IsReward = false,
	bool IsComplete = false,
	bool IsFailed = false,
	List<QuestObjectiveEntry>? Objectives = null
) {
	/// <summary>
	/// Whether the row is expanded. Mutable on purpose: MudTable renders
	/// ChildRowContent from the item instance, so toggling this in place avoids
	/// rebuilding the list (which the parent re-render would otherwise trigger).
	/// </summary>
	public bool IsExpanded { get; set; }

	/// <summary>Objectives for display, never null.</summary>
	public List<QuestObjectiveEntry> ObjectiveList => Objectives ?? [];
}

/// <summary>
/// One objective of a quest, with the TarkovTracker completion state folded in.
/// </summary>
/// <param name="Objective">The task objective from tarkov.dev.</param>
/// <param name="IsComplete">Tracker reports the objective finished.</param>
/// <param name="ProgressCount">Objective counter reported by the tracker, 0 when absent.</param>
public record QuestObjectiveEntry(
	TaskObjective Objective,
	bool IsComplete,
	int ProgressCount
) {
	/// <summary>Amount the objective wants in total.</summary>
	public int Required => Objective.Count;

	/// <summary>True when the objective accepts more than one item.</summary>
	public bool HasAlternates => Objective.ItemIds?.Count > 1;

	/// <summary>Item ids the objective accepts.</summary>
	public IEnumerable<string> ItemIds => Objective.ItemIds ?? [];

	/// <summary>
	/// Whether this objective can be edited. Only true while the local store is
	/// the source.
	/// </summary>
	public bool CanEdit => Item.CanEditLocalProgress;

	public void SetComplete(bool complete) => Item.SetLocalObjectiveComplete(Objective.Id, complete, Required);

	public void Step(int delta) => Item.StepLocalObjective(Objective.Id, delta, Required);

	private static Dictionary<string, Item>? _itemLookup;

	internal static void InvalidateItemLookup() => _itemLookup = null;

	/// <summary>
	/// Every item the objective accepts, resolved against the tarkov.dev
	/// catalogue. Cached because the id lookup would otherwise be a linear
	/// scan over every item, and this renders once per objective per repaint.
	/// </summary>
	public IReadOnlyList<Item> AlternateItems {
		get {
			if (field is not null) {
				return field;
			}

			_itemLookup ??= TarkovDevAPI.GetItems().ToDictionary(i => i.Id, StringComparer.Ordinal);
			var result = new List<Item>();
			foreach (var id in ItemIds) {
				if (_itemLookup.TryGetValue(id, out var item)) {
					result.Add(item);
				}
			}

			field = result;
			return field;
		}
	}

	/// <summary>
	/// Objective text. Uses the upstream description when present, otherwise a
	/// wording derived from the objective type, matching tarkov.dev.
	/// </summary>
	public string Description => !string.IsNullOrWhiteSpace(Objective.Description)
		? Objective.Description
		: Objective.Type switch {
			"findItem" => "Find the items",
			"giveItem" => "Hand over the items",
			"kill" => "Eliminate",
			"visit" => "Visit",
			"extract" => "Extract",
			"survive" => "Survive",
			"craft" => "Craft",
			"sell" => "Sell",
			"buy" => "Buy",
			"repair" => "Repair",
			"equip" => "Equip",
			"loyaltyLevel" => "Reach trader loyalty level",
			"traderReputation" => "Raise trader standing",
			"skillLevel" => "Reach skill level",
			"playerLevel" => "Reach player level",
			"experience" => "Accumulate experience",
			"location" => "Visit location",
			"killstreak" => "Achieve a killstreak",
			"notLoot" => "Avoid taking anything",
			"collect" => "Collect",
			_ => Objective.Type ?? "Objective",
		};
}

public partial class Item {
	/// <summary>
	/// Whether the tracker is the selected source and has returned data. Falls
	/// back to local progress otherwise, including before the first fetch lands.
	/// </summary>
	private static bool UseTarkovTracker =>
		RatConfig.Tracking.Source == RatConfig.ProgressSource.TarkovTracker
		&& RatConfig.Tracking.TarkovTracker.Enable
		&& RatScannerMain.Instance.TarkovTrackerDB.Progress.Count >= 1;

	private static UserProgress GetUserProgress() {
		if (UseTarkovTracker) {
			var teamProgress = RatScannerMain.Instance.TarkovTrackerDB.Progress;
			var tracked = teamProgress.FirstOrDefault(x => x.UserId == RatScannerMain.Instance.TarkovTrackerDB.Self);
			if (tracked != null) return tracked;
		}

		return RatScannerMain.Instance.LocalProgress.GetProgress();
	}

	/// <summary>
	/// The player's own progress. Use <see cref="HasTracker"/> to tell "no
	/// progress recorded" apart from "recorded as not done".
	/// </summary>
	public static UserProgress SelfProgress => GetUserProgress();

	/// <summary>
	/// Whether any progress exists to show, from either source.
	/// </summary>
	public static bool HasTracker => UseTarkovTracker || RatScannerMain.Instance.LocalProgress.HasProgress;

	/// <summary>
	/// TarkovTracker state for a task, or null when the tracker is off, has no
	/// data for the player yet, or has no entry for this task.
	/// </summary>
	public static Progress? GetTaskProgress(TarkovTask task, UserProgress? progress = null) {
		progress ??= GetUserProgress();
		if (progress.Tasks is not { Count: > 0 })
			return null;

		return progress.Tasks.FirstOrDefault(t => t.Id == task.Id && !t.Invalid);
	}

	/// <summary>
	/// Whether the task counts as done: the tracker marks it complete, or every
	/// objective that gates it is complete. Optional objectives never gate.
	/// </summary>
	public static bool IsTaskComplete(TarkovTask task, UserProgress progress) {
		var entry = progress.Tasks?.FirstOrDefault(t => t.Id == task.Id && !t.Invalid);
		if (entry is { Complete: true }) return true;

		if (task.Objectives is not { Count: > 0 }) return false;

		var required = GetObjectiveProgress(task, progress)
			.Where(o => !o.Objective.Optional)
			.ToList();

		return required.Count > 0 && required.All(o => o.IsComplete);
	}

	/// <summary>
	/// Whether TarkovTracker marks the task failed.
	/// </summary>
	public static bool IsTaskFailed(TarkovTask task, UserProgress progress) {
		var entry = progress.Tasks?.FirstOrDefault(t => t.Id == task.Id && !t.Invalid);
		return entry is { Failed: true };
	}

	/// <summary>
	/// Objectives of a task paired with their TarkovTracker completion state and
	/// counter. Objectives with no tracker entry come back as incomplete with a
	/// zero count.
	/// </summary>
	public static List<QuestObjectiveEntry> GetObjectiveProgress(TarkovTask task, UserProgress progress) {
		var result = new List<QuestObjectiveEntry>();
		if (task.Objectives is not { Count: > 0 })
			return result;

		foreach (var objective in task.Objectives) {
			var entry = objective.Id is null
				? null
				: progress.TaskObjectives?.FirstOrDefault(p => p.Id == objective.Id && !p.Invalid);

			result.Add(new QuestObjectiveEntry(objective, entry?.Complete == true, entry?.Count ?? 0));
		}

		return result;
	}

	/// <summary>
	/// Whether progress can be edited here. Only while the local store is the
	/// selected source, since the tracker would overwrite local edits.
	/// </summary>
	public static bool CanEditLocalProgress => !UseTarkovTracker;

	/// <summary>Records an objective's completion in the local store.</summary>
	public static void SetLocalObjectiveComplete(string? objectiveId, bool complete, int required) {
		if (!CanEditLocalProgress) return;
		RatScannerMain.Instance.LocalProgress.SetObjectiveComplete(objectiveId ?? "", complete, required);
	}

	/// <summary>Moves an objective's counter in the local store.</summary>
	public static void StepLocalObjective(string? objectiveId, int delta, int required) {
		if (!CanEditLocalProgress) return;
		RatScannerMain.Instance.LocalProgress.StepObjective(objectiveId ?? "", delta, required);
	}

	/// <summary>Records a task's completion in the local store.</summary>
	public static void SetLocalTaskComplete(string? taskId, bool complete) {
		if (!CanEditLocalProgress) return;
		RatScannerMain.Instance.LocalProgress.SetTaskComplete(taskId ?? "", complete);
	}

	public (int count, int kappaCount) GetTaskRemaining() => GetTaskRemaining(GetUserProgress());
	public (int count, int kappaCount) GetTaskRemaining(UserProgress progress) {
		var report = GetQuestNeedReport(progress);
		return (report.CurrentTotal, report.KappaTotal);
	}

	internal QuestNeedReport GetQuestNeedReport() => GetQuestNeedReport(GetUserProgress());

	/// <summary>
	/// Quests that require handing this item in. Uses findItem/giveItem objectives
	/// with a non-zero remaining count, matching how tarkov.dev builds its quest
	/// table. Optional objectives and the find half of a find/give pair are folded
	/// into the single give objective so a task is listed once.
	/// </summary>
	public List<QuestItemEntry> GetQuestsRequiringItem() {
		var tasks = TarkovDevAPI.GetTasks();
		var result = new List<QuestItemEntry>();

		foreach (var task in tasks) {
			if (task.Objectives is not { Count: > 0 })
				continue;

			var need = GetQuestItemCount(task, Id);
			if (need is null)
				continue;

			result.Add(new QuestItemEntry(task, need.Value, HasAlternates(task, Id)));
		}

		return result;
	}

	/// <summary>
	/// Quests that hand this item out as a completion reward.
	/// </summary>
	public List<QuestItemEntry> GetQuestsRewardingItem() {
		var result = new List<QuestItemEntry>();

		foreach (var task in TarkovDevAPI.GetTasks()) {
			var reward = task.FinishRewards?.Items?.FirstOrDefault(i => i.Item == Id);
			if (reward is null)
				continue;

			result.Add(new QuestItemEntry(task, reward.Count, false, true));
		}

		return result;
	}

	/// <summary>
	/// Remaining count of this item a task still wants handed in, or null when the
	/// task does not need it. A find/give pair for the same items is counted once,
	/// using the give objective, which is the one that actually consumes them.
	/// </summary>
	private static int? GetQuestItemCount(TarkovTask task, string itemId) {
		var objectives = task.Objectives;
		if (objectives is null)
			return null;

		var matchedFind = new HashSet<TaskObjective>();

		foreach (var give in objectives.Where(o =>
			!o.Optional && o.Type == "giveItem" && o.ItemIds?.Contains(itemId) == true
		)) {
			// Tarkov.dev emits find and give as separate objectives for the same
			// physical items; consume the matching find so it is not counted twice.
			foreach (var find in objectives.Where(o =>
				!o.Optional
				&& o.Type == "findItem"
				&& o.Count == give.Count
				&& o.FoundInRaid == give.FoundInRaid
				&& o.ItemIds?.Contains(itemId) == true
			)) {
				_ = matchedFind.Add(find);
			}

			return give.Count;
		}

		// No give objective: a standalone find requirement (e.g. survive-with style
		// objectives) still means the item is needed.
		foreach (var find in objectives.Where(o =>
			!o.Optional && o.Type == "findItem" && o.ItemIds?.Contains(itemId) == true
		)) {
			if (matchedFind.Contains(find))
				continue;
			return find.Count;
		}

		return null;
	}

	/// <summary>
	/// True when the task also asks for other items, so the UI can note that the
	/// displayed amount is one of several accepted requirements.
	/// </summary>
	private static bool HasAlternates(TarkovTask task, string itemId) {
		if (task.Objectives is not { Count: > 0 })
			return false;

		return task.Objectives.Any(o =>
			!o.Optional
			&& (o.Type == "giveItem" || o.Type == "findItem")
			&& o.ItemIds?.Contains(itemId) == true
			&& o.ItemIds.Count > 1
		);
	}

	/// <summary>
	/// Full applicability-aware quest need classification for the item.
	/// Does not inspect the scanned item's FIR state (no vision yet).
	/// </summary>
	internal QuestNeedReport GetQuestNeedReport(UserProgress progress) =>
		QuestNeedClassifier.Classify(this, TarkovDevAPI.GetTasks(), progress, RatConfig.Tracking.ShowNonFIRNeeds);

	/// <summary>
	/// Quest / hideout remaining counts broken down by found-in-raid requirement.
	/// Visual FIR detection on the scanned icon is intentionally not implemented yet.
	/// </summary>
	public readonly record struct RequirementBreakdown(int Total, int FoundInRaid, int NonFoundInRaid) {
		public bool Any => Total > 0;
		public bool HasFirNeed => FoundInRaid > 0;
		public bool HasNonFirNeed => NonFoundInRaid > 0;
	}

	/// <summary>How the player can obtain this item besides looting.</summary>
	public readonly record struct AcquisitionInfo(bool CanCraft, int CraftRecipeCount, bool CanBarter, int BarterOfferCount) {
		public bool Any => CanCraft || CanBarter;
	}

	private int RemainingForObjective(
		TaskObjective objective,
		UserProgress progress,
		bool showNonFir,
		out bool requiresFir
	) {
		requiresFir = false;
		var needed = 0;

		// Optional objectives are nice-to-have, never a requirement.
		if (objective.Optional)
			return 0;

		if (objective.Type is "giveItem" or "findItem" or "plantItem") {
			if (objective.ItemIds == null || !objective.ItemIds.Contains(Id))
				return 0;

			requiresFir = (objective.Type is "giveItem" or "findItem") && objective.FoundInRaid;
			// plantItem is treated as non-FIR requirement (same as legacy).
			if (!showNonFir && !requiresFir)
				return 0;
			if (!showNonFir && objective.Type == "plantItem")
				return 0;

			needed = objective.Count;
			foreach (var p in progress.TaskObjectives.Where(p => p.Id == objective.Id))
				needed -= p.Complete ? objective.Count : p.Count;
			return Math.Max(0, needed);
		}

		if (objective.Type is "mark" or "buildWeapon") {
			if (objective.MarkerItemId != Id && objective.BuildItemId != Id)
				return 0;
			if (!showNonFir)
				return 0;
			requiresFir = false;
			needed = Math.Max(1, objective.Count);
			foreach (var p in progress.TaskObjectives.Where(p => p.Id == objective.Id))
				needed -= 1;
			return Math.Max(0, needed);
		}

		return 0;
	}

	public int GetHideoutRemaining() => GetHideoutRequirementBreakdown(GetUserProgress()).Total;
	public int GetHideoutRemaining(UserProgress progress) => GetHideoutRequirementBreakdown(progress).Total;

	public RequirementBreakdown GetHideoutRequirementBreakdown() => GetHideoutRequirementBreakdown(GetUserProgress());

	/// <summary>
	/// Remaining hideout upgrade needs, split by FIR attribute on the station item requirement.
	/// </summary>
	public RequirementBreakdown GetHideoutRequirementBreakdown(UserProgress progress) {
		var fir = 0;
		var nonFir = 0;
		var stations = TarkovDevAPI.GetHideoutStations();

		foreach (var station in stations) {
			if (station.Levels == null)
				continue;
			foreach (var level in station.Levels) {
				if (level == null)
					continue;

				if (progress.HideoutModules.Any(p => p.Id == level.Id && p.Complete))
					continue;

				if (level.ItemRequirements == null)
					continue;
				foreach (var requiredItem in level.ItemRequirements) {
					if (requiredItem.ItemId != Id)
						continue;

					var remaining = requiredItem.Count;
					foreach (var p in progress.HideoutParts.Where(p => p.Id == requiredItem.Id))
						remaining -= p.Complete ? requiredItem.Count : p.Count;
					remaining = Math.Max(0, remaining);
					if (remaining <= 0)
						continue;

					if (requiredItem.FoundInRaid)
						fir += remaining;
					else
						nonFir += remaining;
				}
			}
		}

		return new RequirementBreakdown(fir + nonFir, fir, nonFir);
	}

	internal static ObjectiveNeedBreakdown GetObjectiveNeedBreakdown(
	Item item,
	IReadOnlyList<TaskObjective>? objectives,
	UserProgress progress,
	bool showNonFir
) {
		var breakdown = GetTaskRequirementBreakdown(
			item,
			objectives,
			progress,
			showNonFir,
			out var weaponHandIn
		);
		return new ObjectiveNeedBreakdown(
			breakdown.Total,
			breakdown.FoundInRaid,
			breakdown.NonFoundInRaid,
			weaponHandIn
		);
	}
	internal static RequirementBreakdown GetTaskRequirementBreakdown(
		Item item,
		IReadOnlyList<TaskObjective>? objectives,
		UserProgress progress,
		bool showNonFir
	) => GetTaskRequirementBreakdown(item, objectives, progress, showNonFir, out _);

	private static RequirementBreakdown GetTaskRequirementBreakdown(
		Item item,
		IReadOnlyList<TaskObjective>? objectives,
		UserProgress progress,
		bool showNonFir,
		out int weaponHandIn
	) {
		weaponHandIn = 0;
		if (objectives == null)
			return new RequirementBreakdown(0, 0, 0);

		var fir = 0;
		var nonFir = 0;
		var isWeapon = item.Types?.Contains("gun", StringComparer.OrdinalIgnoreCase) == true;
		Dictionary<TaskObjective, TaskObjective> pairedFindByGive = [];
		HashSet<TaskObjective> pairedFindObjectives = [];

		foreach (
			var giveObjective in objectives.Where(objective =>
				!objective.Optional && objective.Type == "giveItem" && objective.ItemIds?.Contains(item.Id) == true
			)
		) {
			// Current tarkov.dev find/give pairs share count and FIR flags;
			// loosen this match only if the upstream data starts emitting asymmetric pairs.
			var pairedFind = objectives.FirstOrDefault(candidate =>
				!candidate.Optional
				&& candidate.Type == "findItem"
				&& candidate.Count == giveObjective.Count
				&& candidate.FoundInRaid == giveObjective.FoundInRaid
				&& candidate.ItemIds?.Contains(item.Id) == true
				&& !pairedFindObjectives.Contains(candidate)
			);
			if (pairedFind == null)
				continue;
			pairedFindByGive[giveObjective] = pairedFind;
			_ = pairedFindObjectives.Add(pairedFind);
		}

		foreach (var objective in objectives) {
			// Tarkov exposes "find" and "hand over" as separate objectives for the same
			// physical items. Count the pair once using whichever objective is further along.
			if (pairedFindObjectives.Contains(objective))
				continue;

			var needed = item.RemainingForObjective(objective, progress, showNonFir, out var requiresFir);
			if (pairedFindByGive.TryGetValue(objective, out var pairedFind)) {
				var findRemaining = item.RemainingForObjective(pairedFind, progress, showNonFir, out _);
				needed = Math.Min(needed, findRemaining);
			}
			if (needed <= 0)
				continue;

			if (requiresFir)
				fir += needed;
			else
				nonFir += needed;

			if (isWeapon && objective.Type is "giveItem" or "buildWeapon")
				weaponHandIn += needed;
		}

		return new RequirementBreakdown(fir + nonFir, fir, nonFir);
	}

	public IEnumerable<Item> GetAmmoOfSameCaliber() {
		var caliber = Properties?.Caliber;
		return string.IsNullOrEmpty(caliber)
			? []
			: TarkovDevAPI.GetItems().Where(i => i.Properties?.Caliber == caliber);
	}

	public int GetAvg24hMarketPricePerSlot() {
		var price = Avg24HPrice ?? 0;
		var size = Math.Max(Width * Height, 1);
		return price / size;
	}

	public int GetAvg24hMarketSellProfit() {
		double Ti = 0.03f;
		double Tr = 0.03f;
		double VO = BasePrice ?? 0;
		double VR = Avg24HPrice ?? 0;

		var PO = Math.Log10(VO / VR);
		if (VR < VO) PO = Math.Pow(PO, 1.08);

		var PR = Math.Log10(VR / VO);
		if (VO <= VR) PR = Math.Pow(PR, 1.08);

		var tax = (int)((VO * Ti * Math.Pow(4, PO)) + (VR * Tr * Math.Pow(4, PR)));
		return (Avg24HPrice ?? 0) - tax;
	}

	public TraderPrice? GetBestSellToTraderOffer() => SellToTrader?.MaxBy(i => i.PriceRub);
	public TraderPrice? GetBestBuyFromTraderOffer() => BuyFromTrader?.MinBy(i => i.PriceRub);

	public static Item From(string id) {
		return TarkovDevAPI.GetItems().First(i => i.Id == id);
	}
}
