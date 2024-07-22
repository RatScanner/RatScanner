using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.TarkovDev.Generated;
public class MapPosition
{
	#region members
	[JsonProperty("x")]
	public double X { get; set; }

	[JsonProperty("y")]
	public double Y { get; set; }

	[JsonProperty("z")]
	public double Z { get; set; }
	#endregion
}
