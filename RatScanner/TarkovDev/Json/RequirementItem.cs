using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace RatScanner.TarkovDev.Json;

public class RequirementItem {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("item")]
	public string ItemId { get; set; } = string.Empty;
	public Item? Item => TarkovDevAPI.GetItems().FirstOrDefault(i => i.Id == ItemId);

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("attributes")]
	public Dictionary<string, object>? Attributes { get; set; }

	public bool FoundInRaid => Attributes != null && Attributes.TryGetValue("foundInRaid", out var v) && v is true;
}
