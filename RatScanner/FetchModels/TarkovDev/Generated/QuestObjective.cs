using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class QuestObjective
{
	[Obsolete("Use Task type instead.")]
	[JsonProperty("id")]
	public string Id { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("type")]
	public string Type { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("target")]
	public List<string> Target { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("targetItem")]
	public Item TargetItem { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("number")]
	public int? Number { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("location")]
	public string Location { get; set; }
}
