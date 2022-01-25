using RatEye;
using RatEye.Processing;

namespace RatLib.Scan;

public class ItemNameScan : ItemScan
{
	private Vector2 _toolTipPosition;

	public ItemNameScan(Inspection inspection)
	{
		MatchedItem = inspection.Item;
		Confidence = inspection.MarkerConfidence;
		IconPath = inspection.IconPath;
	}

	public ItemNameScan(Inspection inspection, Vector2 toolTipPosition, int duration)
	{
		MatchedItem = inspection.Item;
		Confidence = inspection.MarkerConfidence;
		IconPath = inspection.IconPath;
		_toolTipPosition = toolTipPosition;
		DissapearAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() + duration;
	}

	public override Vector2 GetToolTipPosition()
	{
		return _toolTipPosition;
	}
}
