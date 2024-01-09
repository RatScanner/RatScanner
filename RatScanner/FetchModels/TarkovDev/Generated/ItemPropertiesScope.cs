using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesScope : ItemProperties
{
	[JsonProperty("ergonomics")]
	public double? Ergonomics { get; set; }

	[JsonProperty("sightModes")]
	public List<int?> SightModes { get; set; }

	[JsonProperty("sightingRange")]
	public int? SightingRange { get; set; }

	[JsonProperty("recoilModifier")]
	public double? RecoilModifier { get; set; }

	[JsonProperty("slots")]
	public List<ItemSlot> Slots { get; set; }

	[JsonProperty("zoomLevels")]
	public List<List<double?>> ZoomLevels { get; set; }
}
