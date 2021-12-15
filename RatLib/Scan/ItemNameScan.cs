using RatStash;
using System.Drawing;

namespace RatLib.Scan
{
	public class ItemNameScan : ItemScan
	{
		private Vector2 MarkerPos;

		public ItemNameScan(Item item)
		{
			MatchedItems = new[] { item };
			IconPath = "https://api.ratscanner.com/v3/file/bitcoin.png";
			ValidItem = true;
		}

		public ItemNameScan(Bitmap capture, Vector2 mousePosition, int scanSize, int duration)
		{
			Capture = capture;
			var inspection = new RatEye.Processing.Inspection(capture);

			if (!inspection.ContainsMarker) return;
			if (inspection.Item == null)
			{
				ValidItem = false;
				return;
			}

			MatchedItems = new[] { inspection.Item };
			Confidence = inspection.MarkerConfidence;
			MarkerPos = mousePosition + new Vector2(inspection.MarkerPosition);
			MarkerPos -= new Vector2(scanSize, scanSize) / 2;
			IconPath = inspection.IconPath;
			ValidItem = true;
			DissapearAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() + duration;
		}

		public override Vector2 GetToolTipPosition()
		{
			return MarkerPos;
		}
	}
}
