using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class ItemPropertiesContainer : ItemProperties
{
	[JsonProperty("capacity")]
	public int? Capacity { get; set; }

	[JsonProperty("grids")]
	public List<ItemStorageGrid> Grids { get; set; }
}
