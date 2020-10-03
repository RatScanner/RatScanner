using System.Drawing;
using RatScanner.FetchModels;

namespace RatScanner.Scan
{
	internal abstract class ItemScan
	{
		internal bool ValidItem = false;

		// This is an array since we can match multiple
		// identical looking items, when using icon scan
		internal MarketItem[] MatchedItems;

		internal Bitmap Capture;

		internal float Confidence;
		internal abstract Vector2 GetToolTipPosition();
	}
}
