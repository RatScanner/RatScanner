using System.Drawing;
using System.IO;
using RatEye.Properties;
using Tesseract;

namespace RatEye
{
	public partial class Config
	{
		public partial class Processing
		{
			/// <summary>
			/// The Inspection class contains parameters, used by the inspection processing module
			/// </summary>
			public class Inspection
			{
				/// <summary>
				/// Marker bitmap to identify regions of interest. This should be a cropped image of the magnifier icon
				/// </summary>
				public Bitmap Marker = new Bitmap(new MemoryStream(Resources.icon_search));

				/// <summary>
				/// Detection threshold of the marker bitmap
				/// </summary>
				public float MarkerThreshold = 0.82f;

				/// <summary>
				/// The scale of the marker used by item inspection windows
				/// </summary>
				public float MarkerItemScale = 16f / 21f;

				/// <summary>
				/// The background color used for the marker if it uses a alpha channel
				/// </summary>
				public Color MarkerBackgroundColor = Color.FromArgb(25, 27, 27);

				/// <summary>
				/// Unscaled width of the box which will be searched for the title
				/// </summary>
				public int BaseTitleSearchWidth = 500;

				/// <summary>
				/// Unscaled height of the box which will be searched for the title
				/// </summary>
				public int BaseTitleSearchHeight = 17;

				/// <summary>
				/// Right padding of the title search box, used when the close button got detected
				/// <para>
				/// This helps to ignore extra buttons like the sort buttons in some container inspection windows.
				/// </para>
				/// See <see cref="CloseButtonColorLowerBound"/> and <see cref="CloseButtonColorUpperBound"/>.
				/// </summary>
				public int BaseTitleSearchRightPadding = 64;

				/// <summary>
				/// The horizontal offset factor of the box which will be searched for the title
				/// <para>
				/// The factor is applied to the scaled width of the <see cref="Marker"/>.
				/// The horizontal offset is originating from the left most edge of the detected marker.
				/// <code>searchBox.left = detectedMarker.left + (Marker.width * Scale)</code>
				/// </para>
				/// </summary>
				public float HorizontalTitleSearchOffsetFactor = 1.2f;

				/// <summary>
				/// Lower bound color to match the close button which is positioned at the top right of inspection windows
				/// </summary>
				public Color CloseButtonColorLowerBound = Color.FromArgb(50, 10, 10);

				/// <summary>
				/// Upper bound color to match the close button which is positioned at the top right of inspection windows
				/// </summary>
				public Color CloseButtonColorUpperBound = Color.FromArgb(80, 15, 15);

				/// <summary>
				/// Tesseract Engine instance used and set by <see cref="RatEye.Processing.Inspection"/>
				/// </summary>
				internal TesseractEngine TesseractEngine;

				/// <summary>
				/// Create a new inspection config instance
				/// </summary>
				public Inspection() { }

				internal string GetHash()
				{
					var components = new string[]
					{
						MarkerThreshold.ToString(),
						MarkerItemScale.ToString(),
						MarkerBackgroundColor.ToString(),
						BaseTitleSearchWidth.ToString(),
						BaseTitleSearchHeight.ToString(),
						BaseTitleSearchRightPadding.ToString(),
						HorizontalTitleSearchOffsetFactor.ToString(),
						CloseButtonColorLowerBound.ToString(),
						CloseButtonColorUpperBound.ToString(),
					};
					return string.Join("<#sep#>", components).SHA256Hash();
				}
			}
		}
	}
}
