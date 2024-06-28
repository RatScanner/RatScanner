using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesMedicalItem : ItemProperties
{
	[JsonProperty("uses")]
	public int? Uses { get; set; }

	[JsonProperty("useTime")]
	public int? UseTime { get; set; }

	[JsonProperty("cures")]
	public List<string> Cures { get; set; }
}
