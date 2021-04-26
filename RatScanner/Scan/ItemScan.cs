using System.Drawing;
using RatStash;

namespace RatScanner.Scan
{
	internal abstract class ItemScan
	{
		internal bool ValidItem = false;

		// This is an array since we can match multiple
		// identical looking items, when using icon scan
		internal Item[] MatchedItems;

		internal Bitmap Capture;

		internal float Confidence;
		internal abstract Vector2 GetToolTipPosition();
	}
}
