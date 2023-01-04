using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatScanner.FetchModels.tarkovdev
{
	public class NeededResponseData
	{
		[JsonProperty("tasks")]
		public List<Task> Tasks { get; set; } = new();

		// Hideout stations
		[JsonProperty("hideoutStations")]
		public List<HideoutStation> HideoutStations { get; set; } = new();

		// Get needed items from tasks and hideout station
		public List<NeededItem> GetNeededItems()
		{
			var neededItems = new List<NeededItem>();
			foreach (var task in Tasks)
			{
				neededItems.AddRange(task.GetNeededItems());
			}
			foreach (var station in HideoutStations)
			{
				neededItems.AddRange(station.GetNeededItems());
			}
			return neededItems;
		}
	}

	public class NeededResponse
	{
		[JsonProperty("data")]
		public NeededResponseData Data { get; set; }

		// Get needed items from data
		public List<NeededItem> GetNeededItems()
		{
			return Data.GetNeededItems();
		}
	}


}
