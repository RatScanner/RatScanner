using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class BossSpawn
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("spawnChance")]
	public double SpawnChance { get; set; }

	[JsonProperty("spawnLocations")]
	public List<BossSpawnLocation> SpawnLocations { get; set; }

	[JsonProperty("escorts")]
	public List<BossEscort> Escorts { get; set; }

	[JsonProperty("spawnTime")]
	public int? SpawnTime { get; set; }

	[JsonProperty("spawnTimeRandom")]
	public bool? SpawnTimeRandom { get; set; }

	[JsonProperty("spawnTrigger")]
	public string SpawnTrigger { get; set; }
}
