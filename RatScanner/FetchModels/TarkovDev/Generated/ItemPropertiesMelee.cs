using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesMelee : ItemProperties
{
	[JsonProperty("slashDamage")]
	public int? SlashDamage { get; set; }

	[JsonProperty("stabDamage")]
	public int? StabDamage { get; set; }

	[JsonProperty("hitRadius")]
	public double? HitRadius { get; set; }
}
