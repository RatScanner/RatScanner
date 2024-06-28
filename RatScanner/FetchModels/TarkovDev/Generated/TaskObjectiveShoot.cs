using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskObjectiveShoot : ITaskObjective
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("maps")]
	public List<Map> Maps { get; set; }

	[JsonProperty("optional")]
	public bool Optional { get; set; }

	[JsonProperty("target")]
	public string Target { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("shotType")]
	public string ShotType { get; set; }

	[JsonProperty("zoneNames")]
	public List<string> ZoneNames { get; set; }

	[JsonProperty("bodyParts")]
	public List<string> BodyParts { get; set; }

	[JsonProperty("usingWeapon")]
	public List<Item> UsingWeapon { get; set; }

	[JsonProperty("usingWeaponMods")]
	public List<List<Item>> UsingWeaponMods { get; set; }

	[JsonProperty("wearing")]
	public List<List<Item>> Wearing { get; set; }

	[JsonProperty("notWearing")]
	public List<Item> NotWearing { get; set; }

	[JsonProperty("distance")]
	public NumberCompare Distance { get; set; }

	[JsonProperty("playerHealthEffect")]
	public HealthEffect PlayerHealthEffect { get; set; }

	[JsonProperty("enemyHealthEffect")]
	public HealthEffect EnemyHealthEffect { get; set; }

	public List<NeededItem> GetNeededItems(string taskId) => new();
}
