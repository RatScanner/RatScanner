using RatEye;
using RatScanner.TarkovDev.Json;

namespace RatScanner.Scan;

public class DefaultItemScan : ItemScan {
	public DefaultItemScan() { }

	public DefaultItemScan(Item item) {
		Item = item;

		string pathEnding = "unknown-item-grid-image.jpg";
		string path = "https://assets.tarkov.dev/" + pathEnding;

		Confidence = 1;
		IconPath = path;
	}

	public override Vector2 GetToolTipPosition() {
		return Vector2.Zero;
	}
}
