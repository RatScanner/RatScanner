using RatLib.Scan;
using RatTracking.FetchModels;
using System.ComponentModel;

namespace RatRazor.Interfaces
{
	public interface IRatScannerUI : INotifyPropertyChanged
	{
		// Item Metadata
		public string Name { get; }
		public string ShortName { get; }
		public string ImageLink { get; }
		public string IconLink { get; }
		public string WikiLink { get; }
		public string ToolsLink { get; }

		// Item Information
		public string Avg24hPrice { get; }
		public string PricePerSlot { get; }
		public string TraderName { get; }
		public string BestTraderPrice { get; }

		public string DiscordLink { get; }
		public string PatreonLink { get; }
		public string GithubLink { get; }

		public List<KeyValuePair<string, NeededItem>> TrackingTeamNeeds { get; }

		public NeededItem TrackingNeeds { get; }
		public NeededItem TrackingTeamNeedsSummed { get; }

		public ItemScan CurrentItemScan { get; }

	}
}
