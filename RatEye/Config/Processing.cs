using System;
using OpenCvSharp.ML;
using OpenCvSharp.Text;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using RatStash;

namespace RatEye
{
	public partial class Config
	{
		/// <summary>
		/// The Processing class contains parameters, which
		/// are shared amongst multiple processing types.
		/// </summary>
		public partial class Processing
		{
			/// <summary>
			/// Use a persistent cache for objects which can be reused
			/// </summary>
			public bool UseCache = true;

			/// <summary>
			/// The language to use when processing
			/// </summary>
			public Language Language = Language.English;

			/// <summary>
			/// Scale of the image. 1f when the image is from a 1080p screen, 2f when 4k, ...
			/// <para><remarks>Use <see cref="Resolution2Scale"/> to compute the scale.</remarks></para>
			/// </summary>
			public float Scale = 1;

			/// <summary>
			/// Inverse scale of the image. 1f when the image is from a 1080p screen, 0.5f when 4k, ...
			/// <code>=> 1f / <see cref="Scale"/></code>
			/// </summary>
			internal float InverseScale => 1f / Scale;

			/// <summary>
			/// The size of a single slot on 1080p resolution, measured in pixel
			/// </summary>
			public float BaseSlotSize = 63;

			/// <summary>
			/// Slot size of a single slot in pixels, considering for scaling
			/// <code>Scale * BaseSlotSize</code>
			/// </summary>
			internal float ScaledSlotSize => Scale * BaseSlotSize;

			/// <summary>
			/// Icon configuration object
			/// </summary>
			public Icon IconConfig = new();

			/// <summary>
			/// Inspection configuration object
			/// </summary>
			public Inspection InspectionConfig = new();

			/// <summary>
			/// Inventory configuration object
			/// </summary>
			public Inventory InventoryConfig = new();

			/// <summary>
			/// Create a new processing config
			/// </summary>
			public Processing() { }

			/// <summary>
			/// Convert a screen resolution to the corresponding scale
			/// </summary>
			/// <param name="width"></param>
			/// <param name="height"></param>
			/// <returns>The scale calculated from the screen resolution</returns>
			public static float Resolution2Scale(int width, int height)
			{
				var screenScaleFactor1 = width / 1920f;
				var screenScaleFactor2 = height / 1080f;

				return Math.Min(screenScaleFactor1, screenScaleFactor2);
			}

			internal string GetHash()
			{
				var components = new string[]
				{
					UseCache.ToString(),
					Language.ToString(),
					Scale.ToString(),
					BaseSlotSize.ToString(),
					IconConfig.GetHash(),
					InspectionConfig.GetHash(),
					InventoryConfig.GetHash(),
				};
				return string.Join("<#sep#>", components).SHA256Hash();
			}
		}
	}
}
