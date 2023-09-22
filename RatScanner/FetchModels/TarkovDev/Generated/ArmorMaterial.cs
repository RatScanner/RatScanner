using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ArmorMaterial
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("destructibility")]
	public double? Destructibility { get; set; }

	[JsonProperty("minRepairDegradation")]
	public double? MinRepairDegradation { get; set; }

	[JsonProperty("maxRepairDegradation")]
	public double? MaxRepairDegradation { get; set; }

	[JsonProperty("explosionDestructibility")]
	public double? ExplosionDestructibility { get; set; }

	[JsonProperty("minRepairKitDegradation")]
	public double? MinRepairKitDegradation { get; set; }

	[JsonProperty("maxRepairKitDegradation")]
	public double? MaxRepairKitDegradation { get; set; }
}
