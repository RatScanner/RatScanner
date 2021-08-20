using System.Drawing;
using RatStash;
using Icon = RatEye.Processing.Icon;

namespace RatScanner.Scan
{
	/*
	 * Ideas to improve performance:
	 *
	 * - Detect item background color and against equals
	 * - Cache item masks
	 */

	internal class ItemIconScan : ItemScan
	{
		internal bool Rotated;
		internal ItemExtraInfo ItemExtraInfo;

		private Icon _icon;
		private Vector2 _mousePosition;

		internal ItemIconScan(Bitmap capture, Vector2 mousePosition)
		{
			Logger.LogDebug($"Use dynamic icons: {RatEye.Config.GlobalConfig.ProcessingConfig.IconConfig.UseDynamicIcons}");
			_mousePosition = mousePosition;
			var inventory = new RatEye.Processing.Inventory(capture);
			_icon = inventory.LocateIcon();
			if (_icon == null) return;

			MatchedItems = new[] { _icon.Item };
			ItemExtraInfo = _icon.ItemExtraInfo;
			Confidence = _icon.DetectionConfidence;
			Rotated = _icon.Rotated;
			IconPath = _icon.IconPath;
			ValidItem = _icon.DetectionConfidence > 0f;
			Logger.LogDebug($"Confidence: {Confidence}");
		}

		internal override Vector2 GetToolTipPosition()
		{
			var pos = _mousePosition;
			pos += new Vector2(_icon.Position + _icon.ItemPosition);
			pos -= new Vector2(RatConfig.IconScan.ScanWidth, RatConfig.IconScan.ScanHeight) / 2;
			return pos;
		}
	}
}
