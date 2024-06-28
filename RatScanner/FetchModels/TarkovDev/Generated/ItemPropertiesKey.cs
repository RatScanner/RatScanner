using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesKey : ItemProperties
{
	[JsonProperty("uses")]
	public int? Uses { get; set; }
}
