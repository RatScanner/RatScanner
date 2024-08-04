using RatEye;
using RatEye.Processing;
using System;

namespace RatScanner.Scan;

public class ItemNameScan : ItemScan {
	private Vector2 _toolTipPosition;

	public ItemNameScan(Inspection inspection) {
		Item = RatScannerMain.Instance.ItemDB[inspection.Item.Id];
		Confidence = inspection.MarkerConfidence;
		IconPath = inspection.IconPath;
	}

	public ItemNameScan(Inspection inspection, Vector2 toolTipPosition, int duration) {
		Item = RatScannerMain.Instance.ItemDB[inspection.Item.Id];
		Confidence = inspection.MarkerConfidence;
		IconPath = inspection.IconPath;
		_toolTipPosition = toolTipPosition;
		DissapearAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() + duration;
	}

	public override Vector2 GetToolTipPosition() {
		return _toolTipPosition;
	}
}
