using System.Drawing;
using RatStash;

namespace RatLib.Scan
{
	public abstract class ItemScan
	{
		public bool ValidItem = false;

		// This is an array since we can match multiple
		// identical looking items, when using icon scan
		public Item[] MatchedItems;

		public Bitmap Capture;

		public float Confidence;

		public string IconPath;

		public long? DissapearAt;

		public abstract Vector2 GetToolTipPosition();

	}
}
