using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RatTracking.FetchModels.tarkovdata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatTracking.FetchModels.tarkovdev
{
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
		[JsonConverter(typeof(ObjectiveConverter))]
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

	public class ObjectiveConverter : JsonConverter
	{
		public override bool CanWrite => false;
		public override bool CanRead => true;
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IObjective);
		}
		public override void WriteJson(JsonWriter writer,
			object value, JsonSerializer serializer)
		{
			throw new InvalidOperationException("Use default serialization.");
		}

		public override object ReadJson(JsonReader reader,
			Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			var jsonObject = JObject.Load(reader);
			var objective = default(IObjective);
			switch (jsonObject.Value<String>("__typename"))
			{
				case "TaskObjectiveItem":
					objective = new ObjectiveItem();
					break;
				case "TaskObjectiveMark":
					objective = new ObjectiveMark();
					break;
				case "TaskObjectiveBuildItem":
					objective = new ObjectiveBuildItem();
					break;
				case "TaskObjectiveShoot":
					objective = new ObjectiveShoot();
					break;
			}
			serializer.Populate(jsonObject.CreateReader(), objective);
			return objective;
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
	public class ObjectiveBuildItem : IObjective
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
			return new List<NeededItem>() { new NeededItem() { Id = Item.Id, Count = 1, FoundInRaid = false, ProgressType = ProgressType.TaskTurnin } }
				.Concat(ContainsAll.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false, ProgressType = ProgressType.TaskTurnin }))
				.Concat(ContainsOne.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false, HasAlternatives = true, ProgressType = ProgressType.TaskTurnin }))
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
					ProgressType = ProgressType.TaskTurnin
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
					FoundInRaid = false,
					ProgressType = ProgressType.TaskTurnin
				}
			};
		}
	}

	// Objective Shoot
	public class ObjectiveShoot : IObjective
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

		// Using Weapons
		[JsonProperty("usingWeapon")]
		public List<Item> UsingWeapon { get; set; }

		// Using Weapon Mods
		[JsonProperty("usingWeaponMods")]
		public List<List<Item>> UsingWeaponMods { get; set; }

		// Wearing
		[JsonProperty("wearing")]
		public List<List<Item>> Wearing { get; set; }


		// Get needed items
		public List<NeededItem> GetNeededItems()
		{
			// Create a list of needed items from the array of weapons
			var neededItems = UsingWeapon.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false, ProgressType = ProgressType.TaskTurnin }).ToList();

			return UsingWeapon.Select(x => new NeededItem() {
				Id = x.Id,
				Count = 1,
				FoundInRaid = false,
				ProgressType = ProgressType.TaskTurnin,
				HasAlternatives = UsingWeapon.Count > 1
			})
			.Concat(UsingWeaponMods.SelectMany(x => x.Select(y => new NeededItem()
			{
				Id = y.Id,
				Count = 1,
				FoundInRaid = false,
				ProgressType = ProgressType.TaskTurnin,
				HasAlternatives = x.Count > 1
			})))
			.Concat(Wearing.SelectMany(x => x.Select(y => new NeededItem()
			{
				Id = y.Id,
				Count = 1,
				FoundInRaid = false,
				ProgressType = ProgressType.TaskTurnin,
				HasAlternatives = x.Count > 1
			})))
			.ToList();
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
			return Keys.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false, HasAlternatives = Keys.Count > 1, ProgressType = ProgressType.TaskKey }).ToList();
		}
	}
}
