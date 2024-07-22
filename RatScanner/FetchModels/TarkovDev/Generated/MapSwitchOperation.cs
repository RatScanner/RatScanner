using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.TarkovDev.Generated;
public class MapSwitchOperation
{
	[JsonProperty("operation")]
	public string Operation { get; set; }

	[JsonProperty("target")]
	public MapSwitchTarget Target { get; set; }
}
