using RatEye;
using RatScanner.TarkovDev.Json;

namespace RatScanner.Scan;

public abstract class ItemScan

// Base Scan Data
{
	public Item Item { get; set; } = new Item();

	public float Confidence { get; set; } = 0;

	public string IconPath { get; set; } = string.Empty;

	public long DissapearAt { get; set; } = 0;

	// Scan tooltip location
	public abstract Vector2 GetToolTipPosition();
}
