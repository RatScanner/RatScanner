using System.Drawing;

namespace RatScanner.Scan
{
	internal class ItemNameScan : ItemScan
	{
		private Vector2 MarkerPos;

		internal ItemNameScan()
		{
			MatchedItems = new[] { RatScannerMain.Instance.ItemDB.GetItem("59faff1d86f7746c51718c9c") };
			IconPath = "https://api.ratscanner.com/v3/file/bitcoin.png";
			ValidItem = true;
		}

		internal ItemNameScan(Bitmap capture, Vector2 mousePosition)
		{
			Capture = capture;
			var inspection = new RatEye.Processing.Inspection(capture);
			Logger.LogDebug(inspection.MarkerConfidence.ToString());

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
			IconPath = inspection.IconPath;
			ValidItem = true;
		}

		internal override Vector2 GetToolTipPosition()
		{
			return MarkerPos;
		}
	}
}
