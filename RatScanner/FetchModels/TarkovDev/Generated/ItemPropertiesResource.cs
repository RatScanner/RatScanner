using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesResource : ItemProperties
{
	[JsonProperty("units")]
	public int? Units { get; set; }
}
