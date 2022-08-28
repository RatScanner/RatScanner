﻿using RatEye;
using RatStash;

namespace RatLib.Scan;

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
		DissapearAt = 0;

		// Item Metadata
		ImageLink = path;
		IconLink = path;
		WikiLink = fake ? "" : "https://escapefromtarkov.fandom.com/wiki/Physical_bitcoin";
		TarkovDevLink = fake ? "" : "https://tarkov.dev/item/physical-bitcoin";

		// Item Information
		Avg24HPrice = 0;
		PricePerSlot = 0;
		TraderName = fake ? "Fence" : "Therapist";
		BestTraderPrice = 0;
	}

	public override Vector2 GetToolTipPosition()
	{
		return Vector2.Zero;
	}
}
