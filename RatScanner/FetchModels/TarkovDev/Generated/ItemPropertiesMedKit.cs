using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesMedKit : ItemProperties
{
	[JsonProperty("hitpoints")]
	public int? Hitpoints { get; set; }

	[JsonProperty("useTime")]
	public int? UseTime { get; set; }

	[JsonProperty("maxHealPerUse")]
	public int? MaxHealPerUse { get; set; }

	[JsonProperty("cures")]
	public List<string> Cures { get; set; }

	[JsonProperty("hpCostLightBleeding")]
	public int? HpCostLightBleeding { get; set; }

	[JsonProperty("hpCostHeavyBleeding")]
	public int? HpCostHeavyBleeding { get; set; }
}
