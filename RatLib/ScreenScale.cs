using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatLib
{
	// Class used to communicate between RatScanner and RatRazor the screens available and their scales
	public class ScreenScale
	{
		public double Scale { get; set; }
		public Rectangle bounds { get; set; }

		public ScreenScale(double scale, Rectangle bounds)
		{
			Scale = scale;
			this.bounds = bounds;
		}

		public bool Equals(ScreenScale screenScale)
		{
			if (screenScale == null) return false;
			if (Scale != screenScale.Scale) return false;
			if (
				bounds.Width != screenScale.bounds.Width || bounds.Height != screenScale.bounds.Height
				) return false;
			return true;
		}
	}
}
