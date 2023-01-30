using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesStim : ItemProperties
{
	[JsonProperty("useTime")]
	public int? UseTime { get; set; }

	[JsonProperty("cures")]
	public List<string> Cures { get; set; }

	[JsonProperty("stimEffects")]
	public List<StimEffect> StimEffects { get; set; }
}
