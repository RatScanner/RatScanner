using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesGlasses : ItemProperties
{
	[JsonProperty("class")]
	public int? Class { get; set; }

	[JsonProperty("durability")]
	public int? Durability { get; set; }

	[JsonProperty("repairCost")]
	public int? RepairCost { get; set; }

	[JsonProperty("blindnessProtection")]
	public double? BlindnessProtection { get; set; }

	[JsonProperty("material")]
	public ArmorMaterial Material { get; set; }
}
