using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatTracking.FetchModels.tarkovdev
{
	public class NeededResponse
	{
		[JsonProperty("tasks")]
		public List<Task> Tasks { get; set; }
	}

	public class Task
	{
		// ID string
		[JsonProperty("id")]
		public string Id { get; set; }
		
		// Name of the task
		[JsonProperty("name")]
		public string Name { get; set; }
		
		// The trader who gives the task
		[JsonProperty("trader")]
		public string Trader { get; set; }
		
		// Minimum player level
		[JsonProperty("minPlayerLevel")]
		public int MinPlayerLevel { get; set; }
		
		// Task requirements
		[JsonProperty("taskRequirements")]
		public List<TaskStatusRequirement> TaskRequirements { get; set; }

		// Objectives
		[JsonProperty("objectives")]
		public List<IObjective> Objectives { get; set; }

		// Faction name
		[JsonProperty("factionName")]
		public string FactionName { get; set; }

		// Needed keys
		[JsonProperty("neededKeys")]
		public List<NeededKey> NeededKeys { get; set; }

		// Get needed items from objectives and needed keys
		public List<NeededItem> GetNeededItems()
		{
			var neededItems = new List<NeededItem>();
			foreach (var objective in Objectives)
			{
				neededItems.AddRange(objective.GetNeededItems());
			}
			foreach (var neededKey in NeededKeys)
			{
				neededItems.AddRange(neededKey.GetNeededItems());
			}
			return neededItems;
		}
	}

	// Trader class
	public class Trader
	{
		// ID string
		[JsonProperty("id")]
		public string Id { get; set; }
		
		// Name of the trader
		[JsonProperty("name")]
		public string Name { get; set; }
	}

	// Task requirements
	public class TaskStatusRequirement
	{
		// Required Task
		[JsonProperty("task")]
		public RequiredTask Task { get; set; }
		
		// Task status
		[JsonProperty("status")]
		public List<string> Status { get; set; } = new List<string>();
	}

	// Required task
	public class RequiredTask
	{
		// ID string
		[JsonProperty("id")]
		public string Id { get; set; }
		
		// Name of the task
		[JsonProperty("name")]
		public string Name { get; set; }
	}

	// Needed Item
	public class NeededItem
	{
		// Item id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Item count
		[JsonProperty("count")]
		public int Count { get; set; }

		// Found in raid
		[JsonProperty("foundInRaid")]
		public bool FoundInRaid { get; set; }

		// Dog tag level
		[JsonProperty("dogTagLevel")]
		public int? DogTagLevel { get; set; } = null;

		// Min durability
		[JsonProperty("minDurability")]
		public int? MinDurability { get; set; } = 0;

		// Max durability
		[JsonProperty("maxDurability")]
		public int? MaxDurability { get; set; } = 100;

		public bool? HasAlternatives { get; set; } = false;
	}

	// Item
	public class Item
	{
		// Item id
		[JsonProperty("id")]
		public string Id { get; set; }
	}

	// Objective interface
	public interface IObjective
	{
		// Objective id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Objective type
		[JsonProperty("type")]
		public string Type { get; set; }

		// Objective optional
		[JsonProperty("optional")]
		public bool Optional { get; set; }

		// Get needed items
		public List<NeededItem> GetNeededItems();
	}

	// Objective BuildItem
	public class BuildItem : IObjective
	{
		// Objective id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Objective type
		[JsonProperty("type")]
		public string Type { get; set; }

		// Objective optional
		[JsonProperty("optional")]
		public bool Optional { get; set; }

		// Item
		[JsonProperty("item")]
		public Item Item { get; set; }

		// Contains all
		[JsonProperty("containsAll")]
		public List<Item> ContainsAll { get; set; }

		// Contains one
		[JsonProperty("containsOne")]
		public List<Item> ContainsOne { get; set; }

		// Get needed items
		public List<NeededItem> GetNeededItems()
		{
			// Return the item and all items in containsAll
			return new List<NeededItem>() { new NeededItem() { Id = Item.Id, Count = 1, FoundInRaid = false } }
				.Concat(ContainsAll.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false }))
				.Concat(ContainsOne.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false, HasAlternatives = true }))
				.ToList();
		}
	}

	// Objective item
	public class ObjectiveItem : IObjective
	{
		// Objective id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Objective type
		[JsonProperty("type")]
		public string Type { get; set; }

		// Objective optional
		[JsonProperty("optional")]
		public bool Optional { get; set; }

		// Objective found in raid
		[JsonProperty("foundInRaid")]
		public bool FoundInRaid { get; set; }

		// Objective dogTagLevel
		[JsonProperty("dogTagLevel")]
		public int DogTagLevel { get; set; }

		// Objective minDurability
		[JsonProperty("minDurability")]
		public int MinDurability { get; set; }

		// Objective maxDurability
		[JsonProperty("maxDurability")]
		public int MaxDurability { get; set; }

		// Item
		[JsonProperty("item")]
		public Item Item { get; set; }

		// Count
		[JsonProperty("count")]
		public int Count { get; set; }

		// Get needed items
		public List<NeededItem> GetNeededItems()
		{
			return new List<NeededItem>()
			{
				new NeededItem()
				{
					Id = Item.Id,
					Count = Count,
					FoundInRaid = FoundInRaid,
					DogTagLevel = DogTagLevel,
					MinDurability = MinDurability,
					MaxDurability = MaxDurability,
				}
			};
		}
	}

	// Objective mark
	public class ObjectiveMark : IObjective
	{
		// Objective id
		[JsonProperty("id")]
		public string Id { get; set; }

		// Objective type
		[JsonProperty("type")]
		public string Type { get; set; }

		// Objective optional
		[JsonProperty("optional")]
		public bool Optional { get; set; }

		// Item
		[JsonProperty("markerItem")]
		public Item Item { get; set; }


		// Get needed items
		public List<NeededItem> GetNeededItems()
		{
			return new List<NeededItem>()
			{
				new NeededItem()
				{
					Id = Item.Id,
					Count = 1,
					FoundInRaid = false
				}
			};
		}
	}

	// Needed Key
	public class NeededKey
	{
		// List of alternative key items
		[JsonProperty("keys")]
		public List<Item> Keys { get; set; }

		// Get needed items - has alternatives if there are multiple items in keys
		public List<NeededItem> GetNeededItems()
		{
			return Keys.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false, HasAlternatives = Keys.Count > 1 }).ToList();
		}
	}
}
