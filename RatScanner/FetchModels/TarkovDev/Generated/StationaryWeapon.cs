using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.TarkovDev.Generated;
public class StationaryWeapon
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("name")]
	public string Mame { get; set; }

	[JsonProperty("shortName")]
	public string ShortName { get; set; }
}
