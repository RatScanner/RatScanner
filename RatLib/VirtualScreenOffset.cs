namespace RatLib;

public class VirtualScreenOffset
{
	public int XOffset { get; }
	public int YOffset { get; }

	public VirtualScreenOffset(int x, int y)
	{
		XOffset = x;
		YOffset = y;
	}
}
