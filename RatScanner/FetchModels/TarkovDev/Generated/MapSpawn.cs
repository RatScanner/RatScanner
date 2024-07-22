using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.TarkovDev.Generated;

public class MapSpawn
{
	[JsonProperty("zoneName")]
	public string ZoneName { get; set; }

	[JsonProperty("position")]
	public MapPosition Position { get; set; }

	[JsonProperty("sides")]
	public List<string> Sides { get; set; }

	[JsonProperty("categories")]
	public List<string> Categories { get; set; }
}
