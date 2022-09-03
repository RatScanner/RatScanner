using System.Drawing;

namespace RatLib;

// Class used to communicate between RatScanner and RatRazor the screens available and their scales
public class ScreenScale
{
	public double Scale { get; set; }
	public Rectangle Bounds { get; set; }

	public ScreenScale(double scale, Rectangle bounds)
	{
		Scale = scale;
		Bounds = bounds;
	}

	public bool Equals(ScreenScale screenScale)
	{
		if (screenScale == null) return false;
		if (Scale != screenScale.Scale) return false;
		if (Bounds.Width != screenScale.Bounds.Width || Bounds.Height != screenScale.Bounds.Height) return false;
		return true;
	}
}
