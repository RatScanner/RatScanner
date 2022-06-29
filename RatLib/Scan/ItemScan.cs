using RatStash;
using RatEye;
using RatTracking;

namespace RatLib.Scan;

public abstract class ItemScan

	// Base Scan Data
{
	public Item MatchedItem;

	public float Confidence;

	public string IconPath;

	public long? DissapearAt;

	// Item Metadata
	public string? ImageLink;
	public string? IconLink;
	public string? WikiLink;
	public string? TarkovDevLink;

	// Item Information
	public int? Avg24hPrice;
	public int? PricePerSlot;
	public string? TraderName;
	public int? BestTraderPrice;

	// Scan tooltip location
	public abstract Vector2 GetToolTipPosition();
}
