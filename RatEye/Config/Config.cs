using RatStash;

namespace RatEye
{
	/// <summary>
	/// Top level config class which includes members for processing, file paths, etc.
	/// </summary>
	public partial class Config
	{
		/// <summary>
		/// Log debug data
		/// </summary>
		public static bool LogDebug = false;

		/// <summary>
		/// Path configuration object
		/// </summary>
		public Path PathConfig = new Path();

		/// <summary>
		/// Processing configuration object
		/// </summary>
		public Processing ProcessingConfig = new Processing();

		/// <summary>
		/// Icon manager
		/// </summary>
		/// <remarks>
		/// Initialized in <see cref="RatEyeEngine"/>
		/// </remarks>
		internal IconManager IconManager;

		/// <summary>
		/// Item database
		/// </summary>
		/// <remarks>
		/// Initialized in <see cref="RatEyeEngine"/>
		/// </remarks>
		internal Database RatStashDB;

		/// <summary>
		/// Create a new config instance
		/// </summary>
		public Config() { }

		internal string GetHash()
		{
			var components = new string[] {
				LogDebug.ToString(),
				PathConfig.GetHash(),
				ProcessingConfig.GetHash(),
			};
			return string.Join("<#sep#>", components).SHA256Hash();
		}
	}
}
