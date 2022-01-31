using RatStash;
using RatEye;
using Icon = RatEye.Processing.Icon;

namespace RatLib.Scan;
/*
* Ideas to improve performance:
*
* - Detect item background color and against equals
* - Cache item masks
*/

public class ItemIconScan : ItemScan
{
	public bool Rotated;
	public ItemExtraInfo ItemExtraInfo;
	public Icon Icon;

	private Vector2 _toolTipPosition;

	public ItemIconScan(Icon icon, Vector2 toolTipPosition, int duration)
	{
		Icon = icon;
		MatchedItem = icon.Item;
		ItemExtraInfo = icon.ItemExtraInfo;
		Confidence = icon.DetectionConfidence;
		Rotated = icon.Rotated;
		IconPath = icon.IconPath;

		_toolTipPosition = toolTipPosition;
		DissapearAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() + duration;
	}

	public override Vector2 GetToolTipPosition()
	{
		return _toolTipPosition;
	}
}
