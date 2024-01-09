using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class HideoutModule
{
	[JsonProperty("level")]
	public int? Level { get; set; }

	[JsonProperty("itemRequirements")]
	public List<ContainedItem> ItemRequirements { get; set; }

	[JsonProperty("moduleRequirements")]
	public List<HideoutModule> ModuleRequirements { get; set; }
}
