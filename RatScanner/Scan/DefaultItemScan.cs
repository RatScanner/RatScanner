using RatEye;

namespace RatScanner.Scan;

public class DefaultItemScan : ItemScan
{
	public DefaultItemScan() { }

	public DefaultItemScan(TarkovDev.GraphQL.Item item)
	{
		Item = item;

		var pathEnding = "unknown-item-grid-image.jpg";
		var path = "https://assets.tarkov.dev/" + pathEnding;

		Confidence = 1;
		IconPath = path;
	}

	public override Vector2 GetToolTipPosition()
	{
		return Vector2.Zero;
	}
}
