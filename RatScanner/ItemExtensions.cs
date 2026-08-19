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

	public (int count, int kappaCount) GetTaskRemaining(UserProgress? progress = null) {
		// Compensation for Damage Tasks
		// These tasks are not tracked by TarkovTracker
		string[] excludedTasks = new string[] {
			"61e6e5e0f5b9633f6719ed95",
			"61e6e60223374d168a4576a6",
			"61e6e621bfeab00251576265",
			"61e6e615eea2935bc018a2c5",
			"61e6e60c5ca3b3783662be27",
		};

		progress ??= GetUserProgress();

		int needed = 0;
		int count = 0;
		int kappaCount = 0;

		bool showNonFir = RatConfig.Tracking.ShowNonFIRNeeds;

		TarkovTask[] tasks = TarkovDevAPI.GetTasks();

		foreach (TarkovTask task in tasks) {
			if (task == null) continue;
			// Skip if task is already completed
			if (progress.Tasks.Any(p => p.Id == task.Id && p.Complete)) continue;

			// Skip if task is to be excluded
			if (excludedTasks.Contains(task.Id)) continue;

			if (task.Objectives == null) continue;
			foreach (TaskObjective? objective in task.Objectives) {
				if (objective == null) continue;
				if (objective.Type == "giveItem") {
					if (!objective.Items.Contains(Id)) continue;	// Skip if item is not the one we are looking for
					if (!showNonFir && !objective.FoundInRaid) continue;					// Skip if item is not FIR
					needed = objective.Count;
					if (task.KappaRequired == true) kappaCount += objective.Count;
					// Subtract amount of already collected items
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= p.Complete ? objective.Count : p.Count;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				} else if (objective.Type == "plantItem") {
					if (!objective.Items.Contains(Id)) continue;	// Skip if item is not the one we are looking for
					if (!showNonFir) continue;										// Skip if item is not FIR
					needed = objective.Count;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= p.Complete ? objective.Count : p.Count;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				} else if (objective.Type == "mark") {
					if (objective.MarkerItem != Id) continue;  // Skip if item is not the one we are looking for
					if (!showNonFir) continue;                      // Skip if item is not FIR
					needed = 1;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= 1;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				} else if (objective.Type == "buildWeapon") {
					if (objective.Item != Id) continue; // Skip if item is not the one we are looking for
					if (!showNonFir) continue;                      // Skip if item is not FIR
					needed = 1;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= 1;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				}
			}
		}
		return (count, kappaCount);
	}

	public int GetHideoutRemaining(UserProgress? progress = null) {
		progress ??= GetUserProgress();
		progress.Tasks ??= new List<Progress>();
		progress.TaskObjectives ??= new List<Progress>();

		int count = 0;
		HideoutStation[] stations = TarkovDevAPI.GetHideoutStations();

		foreach (HideoutStation station in stations) {
			if (station.Levels == null) continue;
			foreach (HideoutStationLevel? level in station.Levels) {
				if (level == null) continue;

				// Skip if level is already built
				if (progress.HideoutModules.Any(p => p.Id == level.Id && p.Complete)) continue;

				if (level?.ItemRequirements == null) continue;
				foreach (RequirementItem? requiredItem in level.ItemRequirements) {
					if (requiredItem?.Item != Id) continue;

					count += requiredItem.Count;
					List<Progress> objectiveProgress = progress.HideoutParts.Where(p => p.Id == requiredItem.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= p.Complete ? requiredItem.Count : p.Count;
				}
			}
		}
		return count;
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
