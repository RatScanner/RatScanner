using RatEye;
using RatStash;
using System;
using Icon = RatEye.Processing.Icon;

namespace RatScanner.Scan;

public class ItemIconScan : ItemScan
{
	public bool Rotated;
	public ItemExtraInfo ItemExtraInfo;
	public Icon Icon;

	private Vector2 _toolTipPosition;

	public ItemIconScan(Icon icon, Vector2 toolTipPosition, int duration)
	{
		Icon = icon;
		Item = RatScannerMain.Instance.ItemDB[icon.Item.Id];
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
