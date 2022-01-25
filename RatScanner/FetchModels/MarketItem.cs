using System;

namespace RatScanner.FetchModels;

[Serializable]
public class MarketItem
{
	// Item data
	public string Id { get; set; }
	public string WikiLink { get; set; } = "Unknown";
	public string ImageLink { get; set; }
	public string IconLink { get; set; }
	public int Timestamp { get; set; }

	// Price data
	public int Price { get; set; }
	public int BasePrice { get; set; }

	// https://youtrack.jetbrains.com/issue/RSRP-468572
	// ReSharper disable InconsistentNaming
	public int Avg24hPrice { get; set; }
	// ReSharper restore InconsistentNaming

	public TraderPrice[] TraderPrices { get; set; }

	public MarketItem(string id)
	{
		Id = id;
	}
}
