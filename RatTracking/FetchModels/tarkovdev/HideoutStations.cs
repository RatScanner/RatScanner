using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatTracking.FetchModels.tarkovdev
{
	public class HideoutStation
	{
		// Id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Name
		[JsonProperty("name")]
		public string Name { get; set; }

		// Station levels
		[JsonProperty("levels")]
		public List<HideoutStationLevel> Levels { get; set; }
	}

	public class HideoutStationLevel
	{
		// Id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Level
		[JsonProperty("level")]
		public int Level { get; set; }

		// Item Requirements
		[JsonProperty("itemRequirements")]
		public List<LevelItemRequirement> ItemRequirements { get; set; }


		// Station level requirements
		[JsonProperty("stationLevelRequirements")]
		public List<LevelStationRequirement> StationLevelRequirements { get; set; }

		// Get needed items
		public List<NeededItem> GetNeededItems()
		{
			var neededItems = new List<NeededItem>();
			foreach (var itemRequirement in ItemRequirements)
			{
				neededItems.AddRange(itemRequirement.GetNeededItems());
			}
			return neededItems;
		}
	}

	public class LevelItemRequirement
	{
		// Id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Item
		[JsonProperty("item")]
		public Item Item { get; set; }

		// Count
		[JsonProperty("count")]
		public int Count { get; set; }

		// Get needed items
		public List<NeededItem> GetNeededItems()
		{
			return new List<NeededItem>
			{
				new NeededItem
				{
					Id = Item.Id,
					Count = Count,
					FoundInRaid = false,
					ProgressType = ProgressType.HideoutTurnin
				}
			};
		}
	}

	public class LevelStationRequirement
	{
		// Id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Station
		[JsonProperty("station")]
		public RequiredStation Station { get; set; }

		// Level
		[JsonProperty("level")]
		public int Level { get; set; }
	}

	public class RequiredStation
	{
		// Id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Name
		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
