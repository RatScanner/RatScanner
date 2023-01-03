using RatStash;
using RatEye;
using RatTracking;

namespace RatLib.Scan;

public abstract class ItemScan

// Base Scan Data
{
	public Item MatchedItem { get; set; }

	public float Confidence { get; set; } = 0;

	public string IconPath { get; set; }

	public long DissapearAt { get; set; } = 0;

	// Item Metadata
	public string? ImageLink { get; set; }
	public string? IconLink { get; set; }
	public string? WikiLink { get; set; }
	public string? TarkovDevLink { get; set; }

	// Item Information
	public int Avg24hPrice { get; set; } = 0;
	public int PricePerSlot { get; set; } = 0;
	public string TraderName { get; set; } = "";
	public int BestTraderPrice { get; set; } = 0;

	// Scan tooltip location
	public abstract Vector2 GetToolTipPosition();
}
