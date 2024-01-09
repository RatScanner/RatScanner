using System;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Ammo
{
	[JsonProperty("item")]
	public Item Item { get; set; }

	[JsonProperty("weight")]
	public double Weight { get; set; }

	[JsonProperty("caliber")]
	public string Caliber { get; set; }

	[JsonProperty("stackMaxSize")]
	public int StackMaxSize { get; set; }

	[JsonProperty("tracer")]
	public bool Tracer { get; set; }

	[JsonProperty("tracerColor")]
	public string TracerColor { get; set; }

	[JsonProperty("ammoType")]
	public string AmmoType { get; set; }

	[JsonProperty("projectileCount")]
	public int? ProjectileCount { get; set; }

	[JsonProperty("damage")]
	public int Damage { get; set; }

	[JsonProperty("armorDamage")]
	public int ArmorDamage { get; set; }

	[JsonProperty("fragmentationChance")]
	public double FragmentationChance { get; set; }

	[JsonProperty("ricochetChance")]
	public double RicochetChance { get; set; }

	[JsonProperty("penetrationChance")]
	public double PenetrationChance { get; set; }

	[JsonProperty("penetrationPower")]
	public int PenetrationPower { get; set; }

	[JsonProperty("accuracyModifier")]
	public double? AccuracyModifier { get; set; }

	[JsonProperty("recoilModifier")]
	public double? RecoilModifier { get; set; }

	[JsonProperty("initialSpeed")]
	public double? InitialSpeed { get; set; }

	[JsonProperty("lightBleedModifier")]
	public double LightBleedModifier { get; set; }

	[JsonProperty("heavyBleedModifier")]
	public double HeavyBleedModifier { get; set; }
}
