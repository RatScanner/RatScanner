using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesMagazine : ItemProperties
{
	[JsonProperty("ergonomics")]
	public double? Ergonomics { get; set; }

	[Obsolete("Use recoilModifier instead.")]
	[JsonProperty("recoil")]
	public double? Recoil { get; set; }

	[JsonProperty("recoilModifier")]
	public double? RecoilModifier { get; set; }

	[JsonProperty("capacity")]
	public int? Capacity { get; set; }

	[JsonProperty("loadModifier")]
	public double? LoadModifier { get; set; }

	[JsonProperty("ammoCheckModifier")]
	public double? AmmoCheckModifier { get; set; }

	[JsonProperty("malfunctionChance")]
	public double? MalfunctionChance { get; set; }

	[JsonProperty("slots")]
	public List<ItemSlot> Slots { get; set; }

	[JsonProperty("allowedAmmo")]
	public List<Item> AllowedAmmo { get; set; }
}
