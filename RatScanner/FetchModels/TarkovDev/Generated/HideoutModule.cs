using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class HideoutModule
{
	[Obsolete("Use HideoutStation type instead.")]
	[JsonProperty("id")]
	public int? Id { get; set; }

	[Obsolete("Use HideoutStation type instead.")]
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("level")]
	public int? Level { get; set; }

	[JsonProperty("itemRequirements")]
	public List<ContainedItem> ItemRequirements { get; set; }

	[JsonProperty("moduleRequirements")]
	public List<HideoutModule> ModuleRequirements { get; set; }
}
