using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesFoodDrink : ItemProperties
{
	[JsonProperty("energy")]
	public int? Energy { get; set; }

	[JsonProperty("hydration")]
	public int? Hydration { get; set; }

	[JsonProperty("units")]
	public int? Units { get; set; }

	[JsonProperty("stimEffects")]
	public List<StimEffect> StimEffects { get; set; }
}
