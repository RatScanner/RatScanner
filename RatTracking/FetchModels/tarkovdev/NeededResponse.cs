using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatTracking.FetchModels.tarkovdev
{
	public class NeededResponseData
	{
		[JsonProperty("tasks")]
		public List<Task> Tasks { get; set; }

		// Hideout stations
		[JsonProperty("hideoutStations")]
		public List<HideoutStation> HideoutStations { get; set; }
	}

	public class NeededResponse
	{
		[JsonProperty("data")]
		public NeededResponseData Data { get; set; }
	}
}
