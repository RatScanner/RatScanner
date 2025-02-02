using RatScanner.FetchModels.TarkovTracker;
using RatScanner.TarkovDev.GraphQL;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RatScanner;

public static class ItemExtensions {
	private static UserProgress GetUserProgress() {
		UserProgress? progress = null;
		if (RatConfig.Tracking.TarkovTracker.Enable && RatScannerMain.Instance.TarkovTrackerDB.Progress.Count >= 1) {
			System.Collections.Generic.List<UserProgress> teamProgress = RatScannerMain.Instance.TarkovTrackerDB.Progress;
			progress = teamProgress.FirstOrDefault(x => x.UserId == RatScannerMain.Instance.TarkovTrackerDB.Self);
		}
		return progress ?? new UserProgress();
	}

	public static int GetTaskRemaining(this Item item, UserProgress? progress = null) {
		return GetTaskItemRemaining(item, progress).Sum(x => x.ItemCount);
	}

	public static ObservableCollection<TaskItemRemaining> GetTaskItemRemaining(this Item item, UserProgress? progress = null) {
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

		bool showNonFir = RatConfig.Tracking.ShowNonFIRNeeds;

		Task[] tasks = TarkovDevAPI.GetTasks();
		ObservableCollection<TaskItemRemaining> taskRemainingItems = new();

		foreach (Task task in tasks) {
			// Skip if task is already completed
			if (progress.Tasks.Any(p => p.Id == task.Id && p.Complete)) continue;

			// Skip if task is to be excluded
			if (excludedTasks.Contains(task.Id)) continue;

			if (task.Objectives == null) continue;

			int count = 0;
			foreach (ITaskObjective? objective in task.Objectives) {
				if (objective == null) continue;
				if (objective is TaskObjectiveItem oGiveItem && oGiveItem.Type == "giveItem") {
					if ((!oGiveItem.Items?.Any(i => i?.Id == item.Id)) ?? true) continue;   // Skip if item is not the one we are looking for
					if (!showNonFir && !oGiveItem.FoundInRaid) continue;                    // Skip if item is not FIR
					count += oGiveItem.Count;
					// Substract amount of already collected items
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= p.Complete ? oGiveItem.Count : p.Count;
				} else if (objective is TaskObjectiveItem oPlantItem && oPlantItem.Type == "plantItem") {
					if ((!oPlantItem.Items?.Any(i => i?.Id == item.Id)) ?? true) continue;  // Skip if item is not the one we are looking for
					if (!showNonFir) continue;                                              // Skip if item is not FIR
					count += oPlantItem.Count;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= p.Complete ? oPlantItem.Count : p.Count;
				} else if (objective is TaskObjectiveMark oMark && oMark.Type == "mark") {
					if (oMark.MarkerItem?.Id != item.Id) continue;  // Skip if item is not the one we are looking for
					if (!showNonFir) continue;                      // Skip if item is not FIR
					count += 1;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= 1;
				} else if (objective is TaskObjectiveBuildItem oBuildWeapon && oBuildWeapon.Type == "buildWeapon") {
					if (oBuildWeapon.Item?.Id != item.Id) continue; // Skip if item is not the one we are looking for
					if (!showNonFir) continue;                      // Skip if item is not FIR
					count += 1;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= 1;
				}
			}

			if (count < 1) continue;
			taskRemainingItems.Add(new TaskItemRemaining(count, task));
		}
		return taskRemainingItems;
	}

	public static ObservableCollection<HideoutItemRemaining> GetHideoutRemainingItem(this Item item, UserProgress? progress = null) {
		progress ??= GetUserProgress();

		ObservableCollection<HideoutItemRemaining> hideoutRemainingItems = new();
		HideoutStation[] stations = TarkovDevAPI.GetHideoutStations();

		foreach (HideoutStation station in stations) {

			if (station.Levels == null) continue;
			foreach (HideoutStationLevel? level in station.Levels) {
				if (level == null) continue;
				int count = 0;

				// Skip if level is already built
				if (progress.HideoutModules.Any(p => p.Id == level.Id && p.Complete)) continue;

				if (level?.ItemRequirements == null) continue;
				foreach (RequirementItem? requiredItem in level.ItemRequirements) {
					if (requiredItem?.Item?.Id != item.Id) continue;
					count += requiredItem.Count;

					List<Progress> objectiveProgress = progress.HideoutParts.Where(p => p.Id == requiredItem.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= p.Complete ? requiredItem.Count : p.Count;
				}

				if (count < 1) continue;
				hideoutRemainingItems.Add(new HideoutItemRemaining(count, station.Name, level.Level));
			}
		}

		return hideoutRemainingItems;
	}

	public static int GetHideoutTotalRemaining(this Item item, UserProgress? progress = null) {
		return GetHideoutRemainingItem(item, progress).Sum(x => x.ItemCount);
	}

	public static int GetAvg24hMarketPricePerSlot(this Item item) {
		int price = item.Avg24HPrice ?? 0;
		int size = item.Width * item.Height;
		return price / size;
	}

	public static ItemPrice? GetBestTraderOffer(this Item item) => item.SellFor?.Where(i => i.Vendor is TraderOffer).MaxBy(i => i.PriceRub);

	public static TraderOffer? GetBestTraderOfferVendor(this Item item) => GetBestTraderOffer(item)?.Vendor as TraderOffer;

	public static IEnumerable<Item> GetAmmoOfSameCaliber(this Item item) {
		if (item.Properties is not ItemPropertiesAmmo ammo) return Enumerable.Empty<Item>();
		return TarkovDevAPI.GetItems().Where(i => i.Properties is ItemPropertiesAmmo a && ammo.Caliber == a.Caliber);
	}
}
