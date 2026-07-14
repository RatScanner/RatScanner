using RatScanner.FetchModels.TarkovTracker;
using RatScanner.TarkovDev.GraphQL;
using System.Collections.Generic;
using System.Linq;

namespace RatScanner.TarkovDev.GraphQL;

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

		Task[] tasks = TarkovDevAPI.GetTasks();

		foreach (Task task in tasks) {
			if (task == null) continue;
			// Skip if task is already completed
			if (progress.Tasks.Any(p => p.Id == task.Id && p.Complete)) continue;

			// Skip if task is to be excluded
			if (excludedTasks.Contains(task.Id)) continue;

			if (task.Objectives == null) continue;
			foreach (ITaskObjective? objective in task.Objectives) {
				if (objective == null) continue;
				if (objective is TaskObjectiveItem oGiveItem && oGiveItem.Type == "giveItem") {
					if ((!oGiveItem.Items?.Any(i => i?.Id == Id)) ?? true) continue;	// Skip if item is not the one we are looking for
					if (!showNonFir && !oGiveItem.FoundInRaid) continue;					// Skip if item is not FIR
					needed = oGiveItem.Count;
					if (task.KappaRequired == true) kappaCount += oGiveItem.Count;
					// Subtract amount of already collected items
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= p.Complete ? oGiveItem.Count : p.Count;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				} else if (objective is TaskObjectiveItem oPlantItem && oPlantItem.Type == "plantItem") {
					if ((!oPlantItem.Items?.Any(i => i?.Id == Id)) ?? true) continue;	// Skip if item is not the one we are looking for
					if (!showNonFir) continue;												// Skip if item is not FIR
					needed = oPlantItem.Count;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= p.Complete ? oPlantItem.Count : p.Count;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				} else if (objective is TaskObjectiveMark oMark && oMark.Type == "mark") {
					if (oMark.MarkerItem?.Id != Id) continue;  // Skip if item is not the one we are looking for
					if (!showNonFir) continue;                      // Skip if item is not FIR
					needed = 1;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= 1;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				} else if (objective is TaskObjectiveBuildItem oBuildWeapon && oBuildWeapon.Type == "buildWeapon") {
					if (oBuildWeapon.Item?.Id != Id) continue; // Skip if item is not the one we are looking for
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
					if (requiredItem?.Item?.Id != Id) continue;

					count += requiredItem.Count;
					List<Progress> objectiveProgress = progress.HideoutParts.Where(p => p.Id == requiredItem.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= p.Complete ? requiredItem.Count : p.Count;
				}
			}
		}
		return count;
	}

	public int GetAvg24hMarketPricePerSlot() {
		int price = Avg24HPrice ?? 0;
		int size = Width * Height;
		return price / size;
	}

	public ItemPrice? GetBestTraderOffer() => SellFor?.Where(i => i.Vendor is TraderOffer).MaxBy(i => i.PriceRub);

	public TraderOffer? GetBestTraderOfferVendor() => GetBestTraderOffer()?.Vendor as TraderOffer;

	public IEnumerable<Item> GetAmmoOfSameCaliber() {
		if (Properties is not ItemPropertiesAmmo ammo) return Enumerable.Empty<Item>();
		return TarkovDevAPI.GetItems().Where(i => i.Properties is ItemPropertiesAmmo a && ammo.Caliber == a.Caliber);
	}

	public static Item From(string id) {
		return TarkovDevAPI.GetItems().First(i => i.Id == id);
	}
}
