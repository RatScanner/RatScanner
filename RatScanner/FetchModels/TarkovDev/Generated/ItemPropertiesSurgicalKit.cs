using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesSurgicalKit : ItemProperties
{
	[JsonProperty("uses")]
	public int? Uses { get; set; }

	[JsonProperty("useTime")]
	public int? UseTime { get; set; }

	[JsonProperty("cures")]
	public List<string> Cures { get; set; }

	[JsonProperty("minLimbHealth")]
	public double? MinLimbHealth { get; set; }

	[JsonProperty("maxLimbHealth")]
	public double? MaxLimbHealth { get; set; }
}
