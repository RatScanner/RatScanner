using RatTracking.FetchModels;
using RatTracking;
using System.ComponentModel;
using RatLib.Scan;
using RatLib;

namespace RatRazor.Interfaces;

public interface IRatScannerUI : INotifyPropertyChanged
{
	public string DiscordLink { get; }
	public string PatreonLink { get; }
	public string GithubLink { get; }

	//public List<KeyValuePair<string, NeededItem>> TrackingTeamNeeds { get; }

	//public NeededItem TrackingNeeds { get; }
	//public NeededItem TrackingTeamNeedsSummed { get; }

	public ItemQueue ItemScans { get; }
	public ItemScan LastItemScan { get; }

	public TarkovTrackerDB TarkovTrackerDB { get; }
	public ProgressDB ProgressDB { get; }

	public List<ScreenScale> ScreenScales { get; }

	public ScreenScale? GameScreenScale { get; }

	public string IntToShortPrice(int? value);

	public string IntToLongPrice(int? value);

	public NeededItem GetItemNeeds(ItemScan item);
	public List<KeyValuePair<string, NeededItem>> GetItemTeamNeeds(ItemScan item);
	public NeededItem GetItemTeamNeedsSummed(ItemScan item);

	
}
