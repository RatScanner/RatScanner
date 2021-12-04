using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.ComponentModel;

namespace RatRazor.Interfaces
{
	public interface IRatScannerUI : INotifyPropertyChanged
	{
		// Item Metadata
		public string Name { get; }
		public string ImageLink { get; }
		public string IconLink { get; }
		public string WikiLink { get; }

		// Item Information
		public string Avg24hPrice { get; }
		public string PricePerSlot { get; }
		public string TraderName { get; }
		public string BestTraderPrice { get; }

		public string DiscordLink { get; }
		public string PatreonLink { get; }
		public string GithubLink { get; }

		public string TrackingNeedsQuestRemaining { get; }
		public string TrackingNeedsHideoutRemaining { get; }

		public void PageSwitcherDragMove();

	}
}
