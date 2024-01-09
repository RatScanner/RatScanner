using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesHeadphone : ItemProperties
{
	[JsonProperty("type")]
	public string Type { get; set; }


	[JsonProperty("ambientVolume")]
	public int? AmbientVolume { get; set; }

	[JsonProperty("compressorAttack")]
	public int? CompressorAttack { get; set; }

	[JsonProperty("compressorGain")]
	public int? CompressorGain { get; set; }

	[JsonProperty("compressorRelease")]
	public int? CompressorRelease { get; set; }

	[JsonProperty("compressorThreshold")]
	public int? CompressorThreshold { get; set; }

	[JsonProperty("compressorVolume")]
	public int? CompressorVolume { get; set; }

	[JsonProperty("cutoffFrequency")]
	public int? CutoffFrequency { get; set; }

	[JsonProperty("distanceModifier")]
	public float? DistanceModifier { get; set; }

	[JsonProperty("distortion")]
	public float? Distortion { get; set; }

	[JsonProperty("dryVolume")]
	public int? DryVolume { get; set; }

	[JsonProperty("highFrequencyGain")]
	public float? HighFrequencyGain { get; set; }

	[JsonProperty("resonance")]
	public float? Resonance { get; set; }
}
