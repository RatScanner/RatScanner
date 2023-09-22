using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesPainkiller : ItemProperties
{
	[JsonProperty("uses")]
	public int? Uses { get; set; }

	[JsonProperty("useTime")]
	public int? UseTime { get; set; }

	[JsonProperty("cures")]
	public List<string> Cures { get; set; }

	[JsonProperty("painkillerDuration")]
	public int? PainkillerDuration { get; set; }

	[JsonProperty("energyImpact")]
	public int? EnergyImpact { get; set; }

	[JsonProperty("hydrationImpact")]
	public int? HydrationImpact { get; set; }
}
