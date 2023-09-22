using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesWeapon : ItemProperties
{
	[JsonProperty("caliber")]
	public string Caliber { get; set; }

	[JsonProperty("defaultAmmo")]
	public Item DefaultAmmo { get; set; }

	[JsonProperty("effectiveDistance")]
	public int? EffectiveDistance { get; set; }

	[JsonProperty("ergonomics")]
	public double? Ergonomics { get; set; }

	[JsonProperty("fireModes")]
	public List<string> FireModes { get; set; }

	[JsonProperty("fireRate")]
	public int? FireRate { get; set; }

	[JsonProperty("maxDurability")]
	public int? MaxDurability { get; set; }

	[JsonProperty("recoilVertical")]
	public int? RecoilVertical { get; set; }

	[JsonProperty("recoilHorizontal")]
	public int? RecoilHorizontal { get; set; }

	[JsonProperty("repairCost")]
	public int? RepairCost { get; set; }

	[JsonProperty("sightingRange")]
	public int? SightingRange { get; set; }

	[JsonProperty("centerOfImpact")]
	public double? CenterOfImpact { get; set; }

	[JsonProperty("deviationCurve")]
	public double? DeviationCurve { get; set; }

	[JsonProperty("deviationMax")]
	public double? DeviationMax { get; set; }

	[JsonProperty("defaultWidth")]
	public int? DefaultWidth { get; set; }

	[JsonProperty("defaultHeight")]
	public int? DefaultHeight { get; set; }

	[JsonProperty("defaultErgonomics")]
	public double? DefaultErgonomics { get; set; }

	[JsonProperty("defaultRecoilVertical")]
	public int? DefaultRecoilVertical { get; set; }

	[JsonProperty("defaultRecoilHorizontal")]
	public int? DefaultRecoilHorizontal { get; set; }

	[JsonProperty("defaultWeight")]
	public double? DefaultWeight { get; set; }

	[JsonProperty("defaultPreset")]
	public Item DefaultPreset { get; set; }

	[JsonProperty("presets")]
	public List<Item> Presets { get; set; }

	[JsonProperty("slots")]
	public List<ItemSlot> Slots { get; set; }

	[JsonProperty("allowedAmmo")]
	public List<Item> AllowedAmmo { get; set; }
}
