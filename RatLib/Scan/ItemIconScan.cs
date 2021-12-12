using RatStash;
using System.Drawing;
using Icon = RatEye.Processing.Icon;

namespace RatLib.Scan
{
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

		public Icon _icon;
		public Vector2 _mousePosition;

		private int _scanWidth;
		private int _scanHeight;

		public ItemIconScan(Bitmap capture, Vector2 mousePosition, int scanWidth, int scanHeight, int duration)
		{
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

			_scanHeight = scanHeight;
			_scanWidth = scanWidth;
			DissapearAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() + duration;
		}

		public override Vector2 GetToolTipPosition()
		{
			var pos = _mousePosition;
			pos += new Vector2(_icon.Position + _icon.ItemPosition);
			pos -= new Vector2(_scanWidth, _scanHeight) / 2;
			return pos;
		}
	}
}
