using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesPreset : ItemProperties
{
	[JsonProperty("baseItem")]
	public Item BaseItem { get; set; }

	[JsonProperty("ergonomics")]
	public double? Ergonomics { get; set; }

	[JsonProperty("recoilVertical")]
	public int? RecoilVertical { get; set; }

	[JsonProperty("recoilHorizontal")]
	public int? RecoilHorizontal { get; set; }

	[JsonProperty("moa")]
	public double? Moa { get; set; }
}
