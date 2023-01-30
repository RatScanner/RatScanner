using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesGrenade : ItemProperties
{
	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("fuse")]
	public double? Fuse { get; set; }

	[JsonProperty("minExplosionDistance")]
	public int? MinExplosionDistance { get; set; }

	[JsonProperty("maxExplosionDistance")]
	public int? MaxExplosionDistance { get; set; }

	[JsonProperty("fragments")]
	public int? Fragments { get; set; }

	[JsonProperty("contusionRadius")]
	public int? ContusionRadius { get; set; }
}
