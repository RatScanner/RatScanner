using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesArmorAttachment : ItemProperties
{
	[JsonProperty("class")]
	public int? Class { get; set; }

	[JsonProperty("durability")]
	public int? Durability { get; set; }

	[JsonProperty("repairCost")]
	public int? RepairCost { get; set; }

	[JsonProperty("speedPenalty")]
	public double? SpeedPenalty { get; set; }

	[JsonProperty("turnPenalty")]
	public double? TurnPenalty { get; set; }

	[JsonProperty("ergoPenalty")]
	public int? ErgoPenalty { get; set; }

	[JsonProperty("headZones")]
	public List<string> HeadZones { get; set; }

	[JsonProperty("material")]
	public ArmorMaterial Material { get; set; }

	[JsonProperty("blindnessProtection")]
	public double? BlindnessProtection { get; set; }
}
