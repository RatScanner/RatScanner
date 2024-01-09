using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesBarrel : ItemProperties
{
	[JsonProperty("ergonomics")]
	public double? Ergonomics { get; set; }

	[JsonProperty("recoilModifier")]
	public double? RecoilModifier { get; set; }

	[JsonProperty("centerOfImpact")]
	public double? CenterOfImpact { get; set; }

	[JsonProperty("deviationCurve")]
	public double? DeviationCurve { get; set; }

	[JsonProperty("deviationMax")]
	public double? DeviationMax { get; set; }

	[JsonProperty("slots")]
	public List<ItemSlot> Slots { get; set; }
}
