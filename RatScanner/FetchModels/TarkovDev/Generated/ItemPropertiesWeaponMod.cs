using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesWeaponMod : ItemProperties
{
	[JsonProperty("ergonomics")]
	public double? Ergonomics { get; set; }

	[Obsolete("Use recoilModifier instead.")]
	[JsonProperty("recoil")]
	public double? Recoil { get; set; }

	[JsonProperty("recoilModifier")]
	public double? RecoilModifier { get; set; }

	[JsonProperty("accuracyModifier")]
	public double? AccuracyModifier { get; set; }

	[JsonProperty("slots")]
	public List<ItemSlot> Slots { get; set; }
}
