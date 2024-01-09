using RatEye;

namespace RatScanner.Scan;

public abstract class ItemScan

// Base Scan Data
{
	public FetchModels.TarkovDev.Item Item { get; set; } = new FetchModels.TarkovDev.Item();

	public float Confidence { get; set; } = 0;

	public string IconPath { get; set; }

	public long DissapearAt { get; set; } = 0;

	// Scan tooltip location
	public abstract Vector2 GetToolTipPosition();
}
