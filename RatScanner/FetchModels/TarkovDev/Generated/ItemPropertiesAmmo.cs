using System;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesAmmo : ItemProperties
{
	[JsonProperty("caliber")]
	public string Caliber { get; set; }

	[JsonProperty("stackMaxSize")]
	public int? StackMaxSize { get; set; }

	[JsonProperty("tracer")]
	public bool? Tracer { get; set; }

	[JsonProperty("tracerColor")]
	public string TracerColor { get; set; }

	[JsonProperty("ammoType")]
	public string AmmoType { get; set; }

	[JsonProperty("projectileCount")]
	public int? ProjectileCount { get; set; }

	[JsonProperty("damage")]
	public int? Damage { get; set; }

	[JsonProperty("armorDamage")]
	public int? ArmorDamage { get; set; }

	[JsonProperty("fragmentationChance")]
	public double? FragmentationChance { get; set; }

	[JsonProperty("ricochetChance")]
	public double? RicochetChance { get; set; }

	[JsonProperty("penetrationChance")]
	public double? PenetrationChance { get; set; }

	[JsonProperty("penetrationPower")]
	public int? PenetrationPower { get; set; }

	[Obsolete("Use accuracyModifier instead.")]
	[JsonProperty("accuracy")]
	public int? Accuracy { get; set; }

	[JsonProperty("accuracyModifier")]
	public double? AccuracyModifier { get; set; }

	[Obsolete("Use recoilModifier instead.")]
	[JsonProperty("recoil")]
	public double? Recoil { get; set; }

	[JsonProperty("recoilModifier")]
	public double? RecoilModifier { get; set; }

	[JsonProperty("initialSpeed")]
	public double? InitialSpeed { get; set; }

	[JsonProperty("lightBleedModifier")]
	public double? LightBleedModifier { get; set; }

	[JsonProperty("heavyBleedModifier")]
	public double? HeavyBleedModifier { get; set; }

	[JsonProperty("durabilityBurnFactor")]
	public double? DurabilityBurnFactor { get; set; }

	[JsonProperty("heatFactor")]
	public double? HeatFactor { get; set; }
}
