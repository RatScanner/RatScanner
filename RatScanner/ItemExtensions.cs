using RatScanner.FetchModels.TarkovTracker;
using RatScanner.TarkovDev.GraphQL;
using System.Collections.Generic;
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

	public static (int count, int kappaCount) GetTaskRemaining(this Item item, UserProgress? progress = null) {
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
			// Skip if task is already completed
			if (progress.Tasks.Any(p => p.Id == task.Id && p.Complete)) continue;

			// Skip if task is to be excluded
			if (excludedTasks.Contains(task.Id)) continue;

			if (task.Objectives == null) continue;
			foreach (ITaskObjective? objective in task.Objectives) {
				if (objective == null) continue;
				if (objective is TaskObjectiveItem oGiveItem && oGiveItem.Type == "giveItem") {
					if ((!oGiveItem.Items?.Any(i => i?.Id == item.Id)) ?? true) continue;	// Skip if item is not the one we are looking for
					if (!showNonFir && !oGiveItem.FoundInRaid) continue;					// Skip if item is not FIR
					needed = oGiveItem.Count;
					if (task.KappaRequired == true) kappaCount += oGiveItem.Count;
					// Subtract amount of already collected items
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= p.Complete ? oGiveItem.Count : p.Count;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				} else if (objective is TaskObjectiveItem oPlantItem && oPlantItem.Type == "plantItem") {
					if ((!oPlantItem.Items?.Any(i => i?.Id == item.Id)) ?? true) continue;	// Skip if item is not the one we are looking for
					if (!showNonFir) continue;												// Skip if item is not FIR
					needed = oPlantItem.Count;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= p.Complete ? oPlantItem.Count : p.Count;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				} else if (objective is TaskObjectiveMark oMark && oMark.Type == "mark") {
					if (oMark.MarkerItem?.Id != item.Id) continue;  // Skip if item is not the one we are looking for
					if (!showNonFir) continue;                      // Skip if item is not FIR
					needed = 1;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) needed -= 1;
					count += needed;
					if (task.KappaRequired == true) kappaCount += needed;
				} else if (objective is TaskObjectiveBuildItem oBuildWeapon && oBuildWeapon.Type == "buildWeapon") {
					if (oBuildWeapon.Item?.Id != item.Id) continue; // Skip if item is not the one we are looking for
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

	public static int GetHideoutRemaining(this Item item, UserProgress? progress = null) {
		progress ??= GetUserProgress();

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
					if (requiredItem?.Item?.Id != item.Id) continue;

					count += requiredItem.Count;
					List<Progress> objectiveProgress = progress.HideoutParts.Where(p => p.Id == requiredItem.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= p.Complete ? requiredItem.Count : p.Count;
				}
			}
		}
		return count;
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

	/// <summary>
	/// Calculates priority score based on timeline of when item will be needed
	/// Higher score = more immediate need
	/// </summary>
	public static int GetPriorityScore(this Item item, UserProgress? progress = null) {
		progress ??= GetUserProgress();
		
		int score = 0;
		
		// Get task and hideout requirements
		var (taskRemaining, kappaCount) = item.GetTaskRemaining(progress);
		int hideoutRemaining = item.GetHideoutRemaining(progress);
		
		if (taskRemaining == 0 && hideoutRemaining == 0) return 0; // Not needed
		
		// Calculate timeline priority
		score += GetTaskTimelinePriority(item, progress, taskRemaining, kappaCount);
		score += GetHideoutTimelinePriority(item, progress, hideoutRemaining);
		
		// Bonus for high value items that are also needed
		if (item.Avg24HPrice > 100000) score += 1;
		
		return score;
	}
	
	private static int GetTaskTimelinePriority(Item item, UserProgress progress, int taskRemaining, int kappaCount) {
		int priority = 0;
		
		if (taskRemaining == 0) return 0;
		
		Task[] tasks = TarkovDevAPI.GetTasks();
		
		foreach (Task task in tasks) {
			// Skip completed tasks
			if (progress.Tasks.Any(p => p.Id == task.Id && p.Complete)) continue;
			
			if (task.Objectives == null) continue;
			foreach (ITaskObjective? objective in task.Objectives) {
				if (objective == null) continue;
				
				bool itemNeeded = false;
				if (objective is TaskObjectiveItem oGiveItem && oGiveItem.Type == "giveItem") {
					itemNeeded = oGiveItem.Items?.Any(i => i?.Id == item.Id) ?? false;
				} else if (objective is TaskObjectiveItem oPlantItem && oPlantItem.Type == "plantItem") {
					itemNeeded = oPlantItem.Items?.Any(i => i?.Id == item.Id) ?? false;
				} else if (objective is TaskObjectiveMark oMark && oMark.Type == "mark") {
					itemNeeded = oMark.MarkerItem?.Id == item.Id;
				} else if (objective is TaskObjectiveBuildItem oBuildWeapon && oBuildWeapon.Type == "buildWeapon") {
					itemNeeded = oBuildWeapon.Item?.Id == item.Id;
				}
				
				if (!itemNeeded) continue;
				
				// Check if this task is currently available (prerequisites met)
				bool prerequisitesMet = AreTaskPrerequisitesMet(task, progress);
				
				if (prerequisitesMet) {
					// Immediate priority - task is available
					priority += task.KappaRequired == true ? 50 : 30;
				} else {
					// Future priority - task is locked
					int dependencyDistance = GetTaskDependencyDistance(task, progress);
					if (dependencyDistance <= 2) {
						priority += task.KappaRequired == true ? 20 : 10;
					} else {
						priority += task.KappaRequired == true ? 10 : 5;
					}
				}
			}
		}
		
		return priority;
	}
	
	private static int GetHideoutTimelinePriority(Item item, UserProgress progress, int hideoutRemaining) {
		int priority = 0;
		
		if (hideoutRemaining == 0) return 0;
		
		HideoutStation[] stations = TarkovDevAPI.GetHideoutStations();
		
		foreach (HideoutStation station in stations) {
			if (station.Levels == null) continue;
			
			foreach (HideoutStationLevel? level in station.Levels) {
				if (level == null) continue;
				
				// Skip completed levels
				if (progress.HideoutModules.Any(p => p.Id == level.Id && p.Complete)) continue;
				
				// Check if this level needs the item
				bool itemNeeded = level?.ItemRequirements?.Any(req => req?.Item?.Id == item.Id) ?? false;
				if (!itemNeeded) continue;
				
				// Check if previous level is completed (prerequisite met)
				bool previousLevelCompleted = IsPreviousHideoutLevelCompleted(station, level, progress);
				
				if (previousLevelCompleted) {
					// Immediate priority - previous level is done
					priority += 25;
				} else {
					// Future priority - need to complete previous levels first
					priority += 10;
				}
			}
		}
		
		return priority;
	}
	
	private static bool AreTaskPrerequisitesMet(Task task, UserProgress progress) {
		if (task.TaskRequirements == null) return true;
		
		foreach (TaskStatusRequirement? req in task.TaskRequirements) {
			if (req?.Task == null) continue;
			
			// Check if prerequisite task is completed
			bool prereqCompleted = progress.Tasks.Any(p => p.Id == req.Task.Id && p.Complete);
			if (!prereqCompleted) return false;
		}
		
		return true;
	}
	
	private static int GetTaskDependencyDistance(Task task, UserProgress progress) {
		// Simple implementation: count how many prerequisite tasks are incomplete
		if (task.TaskRequirements == null) return 0;
		
		int distance = 0;
		foreach (TaskStatusRequirement? req in task.TaskRequirements) {
			if (req?.Task == null) continue;
			
			bool prereqCompleted = progress.Tasks.Any(p => p.Id == req.Task.Id && p.Complete);
			if (!prereqCompleted) distance++;
		}
		
		return distance;
	}
	
	private static bool IsPreviousHideoutLevelCompleted(HideoutStation station, HideoutStationLevel currentLevel, UserProgress progress) {
		if (station.Levels == null) return true;
		
		// Convert to List to enable IndexOf and indexing
		var levelsList = station.Levels.ToList();
		
		// Find the level number of current level
		int currentLevelNum = levelsList.IndexOf(currentLevel);
		if (currentLevelNum <= 0) return true; // First level has no prerequisites
		
		// Check if previous level is completed
		var previousLevel = levelsList[currentLevelNum - 1];
		return progress.HideoutModules.Any(p => p.Id == previousLevel.Id && p.Complete);
	}
}
