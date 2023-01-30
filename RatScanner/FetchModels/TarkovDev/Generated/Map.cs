using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Map
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("tarkovDataId")]
	public string TarkovDataId { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("wiki")]
	public string Wiki { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("enemies")]
	public List<string> Enemies { get; set; }

	[JsonProperty("raidDuration")]
	public int? RaidDuration { get; set; }

	[JsonProperty("players")]
	public string Players { get; set; }

	[JsonProperty("bosses")]
	public List<BossSpawn> Bosses { get; set; }

	[JsonProperty("nameId")]
	public string NameId { get; set; }
}
