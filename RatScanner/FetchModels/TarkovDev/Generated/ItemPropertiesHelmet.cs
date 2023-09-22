using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesHelmet : ItemProperties
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

	[JsonProperty("deafening")]
	public string Deafening { get; set; }

	[JsonProperty("blocksHeadset")]
	public bool? BlocksHeadset { get; set; }

	[JsonProperty("blindnessProtection")]
	public double? BlindnessProtection { get; set; }

	[JsonProperty("slots")]
	public List<ItemSlot> Slots { get; set; }

	[JsonProperty("ricochetX")]
	public double? RicochetX { get; set; }

	[JsonProperty("ricochetY")]
	public double? RicochetY { get; set; }

	[JsonProperty("ricochetZ")]
	public double? RicochetZ { get; set; }
}
