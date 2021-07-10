using System.Drawing;
using RatScanner.Properties;

namespace RatScanner.Scan
{
	internal class ItemNameScan : ItemScan
	{
		private Vector2 MarkerPos;

		internal ItemNameScan()
		{
			MatchedItems = new[] { RatScannerMain.Instance.ItemDB.GetItem("57347baf24597738002c6178") };
			ValidItem = true;
		}

		internal ItemNameScan(Bitmap capture, Vector2 mousePosition)
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
			MarkerPos -= new Vector2(RatConfig.NameScan.MarkerScanSize, RatConfig.NameScan.MarkerScanSize) / 2;
			ValidItem = true;
		}

		internal override Vector2 GetToolTipPosition()
		{
			var position = MarkerPos;
			position += new Vector2(0, Resources.markerFHD.Height);
			position += new Vector2(RatConfig.ToolTip.WidthOffset, RatConfig.ToolTip.HeightOffset);
			return position;
		}
	}
}
