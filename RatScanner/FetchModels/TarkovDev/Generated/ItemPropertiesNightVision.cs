using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesNightVision : ItemProperties
{
	[JsonProperty("intensity")]
	public double? Intensity { get; set; }

	[JsonProperty("noiseIntensity")]
	public double? NoiseIntensity { get; set; }

	[JsonProperty("noiseScale")]
	public double? NoiseScale { get; set; }

	[JsonProperty("diffuseIntensity")]
	public double? DiffuseIntensity { get; set; }
}
