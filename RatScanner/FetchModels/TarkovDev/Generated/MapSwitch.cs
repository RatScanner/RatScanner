using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.TarkovDev.Generated;
public class MapSwitch : MapSwitchTarget
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("switchType")]
	public string SwitchType { get; set; }

	[JsonProperty("activatedBy")]
	public MapSwitch ActivatedBy { get; set; }

	[JsonProperty("activates")]
	public List<MapSwitchOperation> Activates { get; set; }

	[JsonProperty("position")]
	public MapPosition Position { get; set; }
}
