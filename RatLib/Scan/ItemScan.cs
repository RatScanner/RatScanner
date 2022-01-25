using RatStash;
using RatEye;

namespace RatLib.Scan;

public abstract class ItemScan
{
	public Item MatchedItem;

	public float Confidence;

	public string IconPath;

	public long? DissapearAt;

	public abstract Vector2 GetToolTipPosition();
}
