using RatEye;
using RatScanner.FetchModels;
using RatStash;

namespace RatScanner.Scan;

public class DefaultItemScan : ItemScan
{
	public DefaultItemScan() { }

	public DefaultItemScan(bool fake)
	{
		MatchedItem = new Item()
		{
			Id = fake ? "123456789012345678901234" : "59faff1d86f7746c51718c9c",
			Name = fake ? "Loading..." : "Bitcoin",
			ShortName = fake ? "Loading..." : "Bitcoin",
			Width = 1,
			Height = 1,
		};

		var pathEnding = fake ? "unknown-item-grid-image.jpg" : "59faff1d86f7746c51718c9c-base-image.png";
		var path = "https://assets.tarkov.dev/" + pathEnding;

		Confidence = 1;
		IconPath = path;

		// Item Metadata
		ImageLink = path;
		IconLink = path;
		WikiLink = fake ? "" : "https://escapefromtarkov.fandom.com/wiki/Physical_bitcoin";
		TarkovDevLink = fake ? "" : "https://tarkov.dev/item/physical-bitcoin";

		// Item Information
		TraderName = fake ? "Fence" : "Therapist";
	}

	public DefaultItemScan(Item item)
	{
		MatchedItem = item;
		var path = $"https://assets.tarkov.dev/{item.Id}-item-grid-image.jpg";
		Confidence = 1;
		IconPath = path;

		var marketItem = item.GetMarketItem();
		ImageLink = marketItem.ImageLink;
		if (string.IsNullOrWhiteSpace(ImageLink)) ImageLink = "https://assets.tarkov.dev/unknown-item-grid-image.jpg";
		IconLink = marketItem.IconLink;
		if (string.IsNullOrWhiteSpace(IconLink)) IconLink = "https://assets.tarkov.dev/unknown-item-grid-image.jpg";
		WikiLink = marketItem.WikiLink;
		TarkovDevLink = $"https://tarkov.dev/item/{item.Id}";

		Avg24hPrice = marketItem.Avg24hPrice;
		(int w, int h) = item.GetSlotSize();
		PricePerSlot = Avg24hPrice / (w * h);
		(var traderId, BestTraderPrice) = item.GetBestTrader();
		TraderName = TraderPrice.GetTraderName(traderId);
	}

	public override Vector2 GetToolTipPosition()
	{
		return Vector2.Zero;
	}
}
