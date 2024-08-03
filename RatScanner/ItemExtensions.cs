using RatScanner.FetchModels;
using RatScanner.FetchModels.TarkovDev;
using RatScanner.FetchModels.TarkovTracker;
using RatStash;
using System.Linq;
using Item = RatScanner.FetchModels.TarkovDev.Item;

namespace RatScanner;

public static class ItemExtensions
{
	private static UserProgress? GetUserProgress()
	{
		UserProgress progress = null;
		if (RatConfig.Tracking.TarkovTracker.Enable && RatScannerMain.Instance.TarkovTrackerDB.Progress.Count >= 1)
		{
			var teamProgress = RatScannerMain.Instance.TarkovTrackerDB.Progress;
			progress = teamProgress.FirstOrDefault(x => x.UserId == RatScannerMain.Instance.TarkovTrackerDB.Self);
		}
		return progress;
	}

	public static int GetTaskRemaining(this Item item, UserProgress? progress = null)
	{
		return 0;
		// TODO: Reimplement this
		//progress ??= GetUserProgress();

		//var count = 0;
		//var showNonFir = RatConfig.Tracking.ShowNonFIRNeeds;
		//var needed = TarkovDevAPI.GetTasks();
		//var neededItems = needed.Where(i => i.Id == item.Id && (i. || showNonFir));

		//if (progress != null)
		//{
		//	neededItems = neededItems.Where(i => !progress.Tasks.Any(h => h.Id == i.TaskId && h.Complete));
		//}

		//if (!neededItems.Any()) return 0;

		//foreach (var neededItem in neededItems)
		//{
		//	count += neededItem.Count;
		//	if (progress == null) continue;

		//	var taskProgress = progress.TaskObjectives.Where(part => part.Id == neededItem.ProgressId).ToList();
		//	foreach (var p in taskProgress)
		//	{
		//		if (p.Complete) continue;
		//		count -= p.Count;
		//	}
		//}

		//return count;
	}

	public static int GetHideoutRemaining(this Item item, UserProgress? progress = null)
	{
		return 0;
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

	public static int GetAvg24hMarketPricePerSlot(this Item item)
	{
		var price = item.Avg24HPrice.Value;
		var size = item.Width * item.Height;
		return price / size;
	}

	public static ItemPrice? GetBestTraderOffer(this Item item) => item.SellFor.Where(i => i.Vendor is TraderOffer).MaxBy(i => i.PriceRub);

	public static TraderOffer? GetBestTraderOfferVendor(this Item item) => GetBestTraderOffer(item)?.Vendor as TraderOffer;
}
