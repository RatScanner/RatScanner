using RatScanner.FetchModels.TarkovTracker;
using RatScanner.TarkovDev.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RatScanner.TarkovDev.Json;

public partial class Item {
	private static UserProgress GetUserProgress() {
		UserProgress? progress = null;
		if (RatConfig.Tracking.TarkovTracker.Enable && RatScannerMain.Instance.TarkovTrackerDB.Progress.Count >= 1) {
			System.Collections.Generic.List<UserProgress> teamProgress = RatScannerMain.Instance.TarkovTrackerDB.Progress;
			progress = teamProgress.FirstOrDefault(x => x.UserId == RatScannerMain.Instance.TarkovTrackerDB.Self);
		}
		return progress ?? new UserProgress();
	}

	public (int count, int kappaCount) GetTaskRemaining() => GetTaskRemaining(GetUserProgress());
	public (int count, int kappaCount) GetTaskRemaining(UserProgress progress) {
		QuestNeedReport report = GetQuestNeedReport(progress);
		return (report.CurrentTotal, report.KappaTotal);
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
		int needed = 0;

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
			foreach (Progress p in progress.TaskObjectives.Where(p => p.Id == objective.Id))
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
			foreach (Progress p in progress.TaskObjectives.Where(p => p.Id == objective.Id))
				needed -= 1;
			return Math.Max(0, needed);
		}

		return 0;
	}

	public int GetHideoutRemaining() => GetHideoutRequirementBreakdown(GetUserProgress()).Total;
	public int GetHideoutRemaining(UserProgress progress) => GetHideoutRequirementBreakdown(progress).Total;

	/// <summary>
	/// Remaining hideout upgrade needs, split by FIR attribute on the station item requirement.
	/// </summary>
	public RequirementBreakdown GetHideoutRequirementBreakdown(UserProgress progress) {
		int fir = 0;
		int nonFir = 0;
		HideoutStation[] stations = TarkovDevAPI.GetHideoutStations();

		foreach (HideoutStation station in stations) {
			if (station.Levels == null)
				continue;
			foreach (HideoutStationLevel? level in station.Levels) {
				if (level == null)
					continue;

				if (progress.HideoutModules.Any(p => p.Id == level.Id && p.Complete))
					continue;

				if (level.ItemRequirements == null)
					continue;
				foreach (RequirementItem requiredItem in level.ItemRequirements) {
					if (requiredItem.ItemId != Id)
						continue;

					int remaining = requiredItem.Count;
					foreach (Progress p in progress.HideoutParts.Where(p => p.Id == requiredItem.Id))
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
		RequirementBreakdown breakdown = GetTaskRequirementBreakdown(
			item,
			objectives,
			progress,
			showNonFir,
			out int weaponHandIn
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

		int fir = 0;
		int nonFir = 0;
		bool isWeapon = item.Types?.Contains("gun", StringComparer.OrdinalIgnoreCase) == true;
		Dictionary<TaskObjective, TaskObjective> pairedFindByGive = [];
		HashSet<TaskObjective> pairedFindObjectives = [];

		foreach (
			TaskObjective giveObjective in objectives.Where(objective =>
				!objective.Optional && objective.Type == "giveItem" && objective.ItemIds?.Contains(item.Id) == true
			)
		) {
			// Current tarkov.dev find/give pairs share count and FIR flags;
			// loosen this match only if the upstream data starts emitting asymmetric pairs.
			TaskObjective? pairedFind = objectives.FirstOrDefault(candidate =>
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
			pairedFindObjectives.Add(pairedFind);
		}

		foreach (TaskObjective objective in objectives) {
			// Tarkov exposes "find" and "hand over" as separate objectives for the same
			// physical items. Count the pair once using whichever objective is further along.
			if (pairedFindObjectives.Contains(objective))
				continue;

			int needed = item.RemainingForObjective(objective, progress, showNonFir, out bool requiresFir);
			if (pairedFindByGive.TryGetValue(objective, out TaskObjective? pairedFind)) {
				int findRemaining = item.RemainingForObjective(pairedFind, progress, showNonFir, out _);
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
		string? caliber = Properties?.Caliber;
		if (string.IsNullOrEmpty(caliber)) return Enumerable.Empty<Item>();
		return TarkovDevAPI.GetItems().Where(i => i.Properties?.Caliber == caliber);
	}

	public int GetAvg24hMarketPricePerSlot() {
		int price = Avg24HPrice ?? 0;
		int size = Math.Max(Width * Height, 1);
		return price / size;
	}

	public int GetAvg24hMarketSellProfit() {
		return 0;
	}

	public TraderPrice? GetBestSellToTraderOffer() => SellToTrader?.MaxBy(i => i.PriceRub);
	public TraderPrice? GetBestBuyFromTraderOffer() => BuyFromTrader?.MinBy(i => i.PriceRub);

	public static Item From(string id) {
		return TarkovDevAPI.GetItems().First(i => i.Id == id);
	}
}
