using System.Drawing;
using RatEye.Processing;
using Inventory = RatEye.Processing.Inventory;

namespace RatEye
{
	/// <summary>
	/// Core class which allows creating new processing objects
	/// </summary>
	public class RatEyeEngine
	{
		/// <summary>
		/// The config which is used for this <see cref="RatEyeEngine"/>
		/// </summary>
		/// <remarks>Do not modify this config object</remarks>
		public Config Config { get; }

		/// <summary>
		/// Create a <see cref="RatEyeEngine"/> instance which is the basis of all processing
		/// </summary>
		/// <param name="config">The config to use for this instance</param>
		/// <param name="itemDatabase">The <see cref="RatStash.Database"/> which contains all matchable items. For example, if no quest items should be matched, pass a previously filtered <see cref="RatStash.Database"/>.</param>
		/// <remarks>Do not modify the config after passing it</remarks>
		public RatEyeEngine(Config config, RatStash.Database itemDatabase)
		{
			Config = config;

			config.RatStashDB = itemDatabase;

			System.IO.Directory.CreateDirectory(config.PathConfig.CacheDir);

			Config.IconManager = new IconManager(config);
		}

		/// <summary>
		/// Create new <see cref="MultiInspection"/> instance
		/// </summary>
		/// <param name="image">The image to process</param>
		public MultiInspection NewMultiInspection(Bitmap image)
		{
			return new MultiInspection(image, Config);
		}

		/// <summary>
		/// Create new <see cref="Inspection"/> instance
		/// </summary>
		/// <param name="image">The image to process</param>
		public Inspection NewInspection(Bitmap image)
		{
			return new Inspection(image, Config);
		}

		/// <summary>
		/// Create new <see cref="Inventory"/> instance
		/// </summary>
		/// <param name="image">The image to process</param>
		public Inventory NewInventory(Bitmap image)
		{
			return new Inventory(image, Config);
		}
	}
}
