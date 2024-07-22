using MudBlazor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.TarkovDev.Generated;
public class MapExtract : MapSwitchTarget
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("faction")]
	public string Faction { get; set; }

	[JsonProperty("switches")]
	public List<MapSwitch> Switches { get; set; }

	[JsonProperty("position")]
	public MapPosition Position { get; set; }

	[JsonProperty("outline")]
	public List<MapPosition> Outline { get; set; }

	[JsonProperty("top")]
	public double? Top { get; set; }

	[JsonProperty("bottom")]
	public double? Bottom { get; set; }
}
