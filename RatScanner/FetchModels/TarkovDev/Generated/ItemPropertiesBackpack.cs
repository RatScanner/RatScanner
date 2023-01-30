using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesBackpack : ItemProperties
{
	[JsonProperty("capacity")]
	public int? Capacity { get; set; }

	[JsonProperty("grids")]
	public List<ItemStorageGrid> Grids { get; set; }

	[Obsolete("Use grids instead.")]
	[JsonProperty("pouches")]
	public List<ItemStorageGrid> Pouches { get; set; }
}
