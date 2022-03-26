using RatTracking.FetchModels;
using System.ComponentModel;
using RatLib.Scan;

namespace RatRazor.Interfaces;

public interface IRatScannerUI : INotifyPropertyChanged
{
	// Item Metadata
	public string Name { get; }
	public string ShortName { get; }
	public string ImageLink { get; }
	public string IconLink { get; }
	public string WikiLink { get; }
	public string TarkovDevLink { get; }

	// Item Information
	public int Avg24hPrice { get; }
	public int PricePerSlot { get; }
	public string TraderName { get; }
	public int BestTraderPrice { get; }

	public string DiscordLink { get; }
	public string PatreonLink { get; }
	public string GithubLink { get; }

	public List<KeyValuePair<string, NeededItem>> TrackingTeamNeeds { get; }

	public NeededItem TrackingNeeds { get; }
	public NeededItem TrackingTeamNeedsSummed { get; }

	public ItemScan CurrentItemScan { get; }

	public string IntToShortPrice(int? value);

	public string IntToLongPrice(int? value);
}
