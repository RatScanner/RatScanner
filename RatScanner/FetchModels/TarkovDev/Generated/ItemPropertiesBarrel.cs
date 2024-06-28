using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesBarrel : ItemProperties
{
	[JsonProperty("ergonomics")]
	public double? Ergonomics { get; set; }

	[Obsolete("Use recoilModifier instead.")]
	[JsonProperty("recoil")]
	public double? Recoil { get; set; }

	[JsonProperty("recoilModifier")]
	public double? RecoilModifier { get; set; }

	[Obsolete("Use centerOfImpact, deviationCurve, and deviationMax instead.")]
	[JsonProperty("accuracyModifier")]
	public double? AccuracyModifier { get; set; }

	[JsonProperty("centerOfImpact")]
	public double? CenterOfImpact { get; set; }

	[JsonProperty("deviationCurve")]
	public double? DeviationCurve { get; set; }

	[JsonProperty("deviationMax")]
	public double? DeviationMax { get; set; }

	[JsonProperty("slots")]
	public List<ItemSlot> Slots { get; set; }
}
