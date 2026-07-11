using OpenCvSharp;

namespace RatEye
{
	public partial class Config
	{
		public partial class Processing
		{
			/// <summary>
			/// The Inventory class contains parameters, used by the inventory processing module.
			/// </summary>
			public class Inventory
			{
				/// <summary>
				/// Color of the grid as defined by EFT
				/// </summary>
				internal Scalar GridColor => new(84, 81, 73, 255);

				/// <summary>
				/// Alpha value of item background colors as defined by EFT
				/// </summary>
				internal int BackgroundAlpha => 77;

				/// <summary>
				/// Minimum color for thresholding the grid
				/// </summary>
				public (int hue, int saturation, int value) MinGridColor = (100, 15, 63);

				/// <summary>
				/// Maximum color for thresholding the grid
				/// </summary>
				public (int hue, int saturation, int value) MaxGridColor = (146, 46, 96);

				/// <summary>
				/// Minimum color for thresholding the highlighting background of an item
				/// </summary>
				public (int hue, int saturation, int value) MinHighlightingColor = (0, 0, 80);

				/// <summary>
				/// Maximum color for thresholding the highlighting background of an item
				/// </summary>
				public (int hue, int saturation, int value) MaxHighlightingColor = (255, 3, 100);

				/// <summary>
				/// If <see langword="true"/>, all processing will be optimized for highlighted items
				/// </summary>
				public bool OptimizeHighlighted = false;

				/// <summary>
				/// Create a new inventory config instance
				/// </summary>
				public Inventory() { }

				internal string GetHash()
				{
					var components = new string[]
					{
						GridColor.ToString(),
						BackgroundAlpha.ToString(),
						MinGridColor.ToString(),
						MaxGridColor.ToString(),
						MinHighlightingColor.ToString(),
						MaxHighlightingColor.ToString(),
						OptimizeHighlighted.ToString(),
					};
					return string.Join("<#sep#>", components).SHA256Hash();
				}
			}
		}
	}
}
