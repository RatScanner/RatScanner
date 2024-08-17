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

	public static int GetTaskRemaining(this Item item, UserProgress? progress = null) {
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

		int count = 0;
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
					if (oGiveItem.Item?.Id != item.Id) continue;    // Skip if item is not the one we are looking for
					count += oGiveItem.Count;                       // Add amount of items needed
																	// Substract amount of already collected items
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= p.Complete ? oGiveItem.Count : p.Count;
				} else if (objective is TaskObjectiveItem oPlantItem && oPlantItem.Type == "plantItem") {
					if (oPlantItem.Item?.Id != item.Id) continue;
					count += oPlantItem.Count;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= p.Complete ? oPlantItem.Count : p.Count;
				} else if (objective is TaskObjectiveMark oMark && oMark.Type == "mark") {
					if (oMark.MarkerItem?.Id != item.Id) continue;
					count += 1;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= 1;
				} else if (objective is TaskObjectiveBuildItem oBuildWeapon && oBuildWeapon.Type == "buildWeapon") {
					if (oBuildWeapon.Item?.Id != item.Id) continue;
					count += 1;
					List<Progress> objectiveProgress = progress.TaskObjectives.Where(p => p.Id == objective.Id).ToList();
					foreach (Progress p in objectiveProgress) count -= 1;
				}
			}
		}
		return count;
	}


#pragma warning disable IDE0060 // Remove unused parameter
	public static int GetHideoutRemaining(this Item item, UserProgress? progress = null)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		return 4;
		// TODO: Reimplement this
		//progress ??= GetUserProgress();

		//var count = 0;
		//var needed = TarkovDevAPI.GetNeededItems().hideout;
		//var neededItems = needed.Where(i => i.Id == item.Id);

		//if (progress != null)
		//{
		//	neededItems = neededItems.Where(i => !progress.HideoutModules.Any(h => h.Id == i.ModuleId && h.Complete));
		//}

		//if (!neededItems.Any()) return 0;

		//foreach (var neededItem in neededItems)
		//{
		//	count += neededItem.Count;
		//	if (progress == null) continue;

		//	var partProgress = progress.HideoutParts.Where(part => part.Id == neededItem.ProgressId).ToList();
		//	foreach (var p in partProgress)
		//	{
		//		if (p.Complete) continue;
		//		count -= p.Count;
		//	}
		//}

		//return count;
	}

	public static int GetAvg24hMarketPricePerSlot(this Item item) {
		int price = item.Avg24HPrice ?? 0;
		int size = item.Width * item.Height;
		return price / size;
	}

	public static ItemPrice? GetBestTraderOffer(this Item item) => item.SellFor?.Where(i => i.Vendor is TraderOffer).MaxBy(i => i.PriceRub);

	public static TraderOffer? GetBestTraderOfferVendor(this Item item) => GetBestTraderOffer(item)?.Vendor as TraderOffer;
}
