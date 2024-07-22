using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.TarkovDev.Generated;
public class Lock
{
	[JsonProperty("lockType")]
	public string LockType { get; set; }

	[JsonProperty("key")]
	public Item Key { get; set; }

	[JsonProperty("needsPower")]
	public bool? NeedsPower { get; set; }

	[JsonProperty("position")]
	public MapPosition Position { get; set; }

	[JsonProperty("outline")]
	public List<MapPosition> Outline { get; set; }

	[JsonProperty("top")]
	public double? Top { get; set; }

	[JsonProperty("bottom")]
	public double? Bottom { get; set; }
}
