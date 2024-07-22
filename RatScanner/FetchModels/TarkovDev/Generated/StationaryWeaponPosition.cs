using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.TarkovDev.Generated;
public class StationaryWeaponPosition
{
	[JsonProperty("stationaryWeapon")]
	public StationaryWeapon StationaryWeapon { get; set; }

	[JsonProperty("position")]
	public MapPosition Position { get; set; }
}
