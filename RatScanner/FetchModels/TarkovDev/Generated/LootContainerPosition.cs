using Newtonsoft.Json;
using RatStash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.TarkovDev.Generated;
public class LootContainerPosition
{
	[JsonProperty("lootContainer")]
	public LootContainer LootContainer { get; set; }

	[JsonProperty("position")]
	public MapPosition Position { get; set; }
}
