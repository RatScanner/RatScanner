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
		public Trader Trader { get; set; }

		// Minimum player level
		[JsonProperty("minPlayerLevel")]
		public int MinPlayerLevel { get; set; }
		
		// Task requirements
		[JsonProperty("taskRequirements")]
		public List<TaskStatusRequirement> TaskRequirements { get; set; } = new();

		// Objectives
		[JsonProperty("objectives")]
		public List<IObjective> Objectives { get; set; } = new();

		// Faction name
		[JsonProperty("factionName")]
		public string FactionName { get; set; }

		// Needed keys
		[JsonProperty("neededKeys")]
		public List<NeededKey> NeededKeys { get; set; } = new();

		// Get needed items from objectives and needed keys
		public List<NeededItem> GetNeededItems()
		{
			var neededItems = new List<NeededItem>();
			foreach (var objective in Objectives)
			{
				neededItems.AddRange(objective.GetNeededItems(Id));
			}
			foreach (var neededKey in NeededKeys)
			{
				neededItems.AddRange(neededKey.GetNeededItems(Id));
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
				default:
					objective = new ObjectiveOther();
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
	[JsonConverter(typeof(ObjectiveConverter))]
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
		public List<NeededItem> GetNeededItems(string taskId);
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
		public List<Item> ContainsAll { get; set; } = new();

		// Contains one
		[JsonProperty("containsOne")]
		public List<Item> ContainsOne { get; set; } = new();

		// Get needed items
		public List<NeededItem> GetNeededItems(string taskId)
		{
			// Return the item and all items in containsAll
			return new List<NeededItem>() { new NeededItem() { Id = Item.Id, Count = 1, FoundInRaid = false, ProgressType = ProgressType.TaskTurnin, ProgressId = Id, TaskId = taskId } }
				.Concat(ContainsAll.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false, ProgressType = ProgressType.TaskTurnin, ProgressId = Id, TaskId = taskId }))
				.Concat(ContainsOne.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false, HasAlternatives = true, ProgressType = ProgressType.TaskTurnin, ProgressId = Id, TaskId = taskId }))
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
		public List<NeededItem> GetNeededItems(string taskId)
		{
			// If type is 'giveItem' return the needed item, else return an empty list
			if (Type == "giveItem")
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
						ProgressType = ProgressType.TaskTurnin,
						ProgressId = Id,
						TaskId = taskId
					}
				};
			}
			else
			{
				return new List<NeededItem>();
			}
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
		public List<NeededItem> GetNeededItems(string taskId)
		{
			return new List<NeededItem>()
			{
				new NeededItem()
				{
					Id = Item.Id,
					Count = 1,
					FoundInRaid = false,
					ProgressType = ProgressType.TaskTurnin,
					ProgressId = Id,
					TaskId = taskId
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
		public List<Item> UsingWeapon { get; set; } = new();

		// Using Weapon Mods
		[JsonProperty("usingWeaponMods")]
		public List<List<Item>> UsingWeaponMods { get; set; } = new();

		// Wearing
		[JsonProperty("wearing")]
		public List<List<Item>> Wearing { get; set; } = new();


		// Get needed items
		public List<NeededItem> GetNeededItems(string taskId)
		{
			// Create a list of needed items from the array of weapons
			return UsingWeapon.Select(x => new NeededItem() {
				Id = x.Id,
				Count = 1,
				FoundInRaid = false,
				ProgressType = ProgressType.TaskUse,
				HasAlternatives = UsingWeapon.Count > 1,
				ProgressId = Id,
				TaskId = taskId
			})
			.Concat(UsingWeaponMods.SelectMany(x => x.Select(y => new NeededItem()
			{
				Id = y.Id,
				Count = 1,
				FoundInRaid = false,
				ProgressType = ProgressType.TaskUse,
				HasAlternatives = x.Count > 1,
				ProgressId = Id,
				TaskId = taskId
			})))
			.Concat(Wearing.SelectMany(x => x.Select(y => new NeededItem()
			{
				Id = y.Id,
				Count = 1,
				FoundInRaid = false,
				ProgressType = ProgressType.TaskUse,
				HasAlternatives = x.Count > 1,
				ProgressId = Id,
				TaskId = taskId
			})))
			.ToList();
		}
	}

	// Objective Other
	public class ObjectiveOther : IObjective
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
		public List<NeededItem> GetNeededItems(string taskId)
		{
			return new List<NeededItem>();
		}
	}

	// Needed Key
	public class NeededKey
	{
		// List of alternative key items
		[JsonProperty("keys")]
		public List<Item> Keys { get; set; }

		// Get needed items - has alternatives if there are multiple items in keys
		public List<NeededItem> GetNeededItems(string taskId)
		{
			return Keys.Select(x => new NeededItem() { Id = x.Id, Count = 1, FoundInRaid = false, HasAlternatives = Keys.Count > 1, ProgressType = ProgressType.TaskKey, TaskId = taskId }).ToList();
		}
	}
}
