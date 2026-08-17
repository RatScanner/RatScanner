using Newtonsoft.Json;

namespace RatScanner.TarkovDev.Json;

public class ItemProperties {
	[JsonProperty("propertiesType")]
	public string PropertiesType { get; set; } = string.Empty;

	[JsonProperty("caliber")]
	public string? Caliber { get; set; }

	[JsonProperty("damage")]
	public int? Damage { get; set; }

	[JsonProperty("penetrationPower")]
	public int? PenetrationPower { get; set; }

	[JsonProperty("fragmentationChance")]
	public decimal? FragmentationChance { get; set; }

	[JsonProperty("projectileCount")]
	public int? ProjectileCount { get; set; }

	[JsonProperty("armorDamage")]
	public int? ArmorDamage { get; set; }
}
