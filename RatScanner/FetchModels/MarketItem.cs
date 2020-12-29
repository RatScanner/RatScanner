using System;
using System.Linq;

namespace RatScanner.FetchModels
{
	[Serializable]
	public class MarketItem
	{
		// Item data
		public string Uid { get; set; }
		public string Name { get; set; } = "Unknown";
		public string ShortName { get; set; } = "Unknown";
		public int Slots { get; set; } = 1;
		public string WikiLink { get; set; } = "Unknown";
		public string ImgLink { get; set; } = "Unknown";
		public int Timestamp { get; set; } = 0;

		public Boolean Quest { get; set; } = false;
		public int QuestCount { get; set; } = -1;

		public Boolean QuestInRaid { get; set; } = false;

		// Price data
		public int Price { get; set; } = 0;

		// https://youtrack.jetbrains.com/issue/RSRP-468572
		// ReSharper disable InconsistentNaming
		public int Avg24hPrice { get; set; } = 0;
		public int Avg7dPrice { get; set; } = 0;
		public int Avg24hAgo { get; set; } = 0;
		public int Avg7dAgo { get; set; } = 0;
		// ReSharper restore InconsistentNaming

		public string TraderName { get; set; } = "Unknown";
		public int TraderPrice { get; set; } = 0;
		public string TraderCurrency { get; set; } = "Unknown";

		/// <summary>
		/// Mods on the item as array of sub item.
		/// For example if the MarketItem is a Weapon and
		/// it has a barrel, grip, stock and scope attached,
		/// those uid's would be in this array. 
		/// </summary>
		/// <remarks>
		/// Contract is not to have mods inside mods as of now.
		/// </remarks>
		public MarketItem[] Mods { get; set; }

		/// <summary>
		/// Metadata to distinguish items with folded stocks or other states
		/// </summary>
		public string Meta { get; set; }

		public bool HasMods => Mods?.Length > 0;

		public MarketItem(string uid)
		{
			Uid = uid;
		}

		public static implicit operator ItemInfo(MarketItem marketItem)
		{
			var meta = marketItem.Meta;

			if (!(marketItem.HasMods)) return new ItemInfo(marketItem.Uid, null, meta);

			var mods = marketItem.Mods.Select(mod => mod.Uid).ToArray();
			return new ItemInfo(marketItem.Uid, mods, meta);
		}

		/// <summary>
		/// Calculate the sum over a property including the items mods
		/// </summary>
		/// <param name="func">Lambda function to determine the used property</param>
		/// <returns>Sum over property of item including mods</returns>
		public int SumMods(Func<MarketItem, int> func)
		{
			var sum = func(this);
			if (HasMods) sum += Mods.Select(func).Sum();
			return sum;
		}

		/// <summary>
		/// Calculate the minimum over a property including the items mods
		/// </summary>
		/// <param name="func">Lambda function to determine the used property</param>
		/// <returns>Min over property of item including mods</returns>
		public int MinMods(Func<MarketItem, int> func)
		{
			var min = func(this);
			var minMods = min;
			if (HasMods) minMods = Mods.Select(func).Min();
			return min < minMods ? min : minMods;
		}
	}


}
