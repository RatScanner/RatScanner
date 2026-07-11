using Tesseract;

namespace RatEye
{
	public partial class Config
	{
		public partial class Processing
		{
			/// <summary>
			/// The Icon class contains parameters, used by the icon processing module.
			/// </summary>
			public class Icon
			{
				/// <summary>
				/// Different kinds of scan modes which can be used to scan an icon
				/// </summary>
				public enum ScanModes
				{
					/// <summary>
					/// Scan the icon by matching it against a template image of every icon
					/// </summary>
					TemplateMatching,
					/// <summary>
					/// Scan the icon by recognizing the short name in the top right corner
					/// </summary>
					OCR,
				}

				/// <summary>
				/// The scanning mode used for icon scanning
				/// </summary>
				public ScanModes ScanMode = Icon.ScanModes.TemplateMatching;

				/// <summary>
				/// Use the static icons for template matching
				/// </summary>
				public bool UseStaticIcons = false;

				/// <summary>
				/// Scan for 90° rotated icons
				/// </summary>
				public bool ScanRotatedIcons = true;

				/// <summary>
				/// Tesseract Engine instance used and set by <see cref="RatEye.Processing.Icon"/>
				/// </summary>
				internal TesseractEngine TesseractEngine;

				/// <summary>
				/// Create a new icon config instance
				/// </summary>
				public Icon() { }

				internal string GetHash()
				{
					var components = new string[]
					{
						ScanMode.ToString(),
						UseStaticIcons.ToString(),
						ScanRotatedIcons.ToString(),
					};
					return string.Join("<#sep#>", components).SHA256Hash();
				}
			}
		}
	}
}
