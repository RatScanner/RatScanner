using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Task
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("tarkovDataId")]
	public int? TarkovDataId { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; }

	[JsonProperty("trader")]
	public Trader Trader { get; set; }

	[JsonProperty("map")]
	public Map Map { get; set; }

	[JsonProperty("experience")]
	public int Experience { get; set; }

	[JsonProperty("wikiLink")]
	public string WikiLink { get; set; }

	[JsonProperty("minPlayerLevel")]
	public int? MinPlayerLevel { get; set; }

	[JsonProperty("taskRequirements")]
	public List<TaskStatusRequirement> TaskRequirements { get; set; }

	[JsonProperty("traderLevelRequirements")]
	public List<RequirementTrader> TraderLevelRequirements { get; set; }

	[JsonProperty("objectives")]
	public List<ITaskObjective> Objectives { get; set; }

	[JsonProperty("startRewards")]
	public TaskRewards StartRewards { get; set; }

	[JsonProperty("finishRewards")]
	public TaskRewards FinishRewards { get; set; }

	[JsonProperty("factionName")]
	public string FactionName { get; set; }

	[JsonProperty("neededKeys")]
	public List<TaskKey> NeededKeys { get; set; }

	[JsonProperty("descriptionMessageId")]
	public string DescriptionMessageId { get; set; }

	[JsonProperty("startMessageId")]
	public string StartMessageId { get; set; }

	[JsonProperty("successMessageId")]
	public string SuccessMessageId { get; set; }

	[JsonProperty("failMessageId")]
	public string FailMessageId { get; set; }

	public List<NeededItem> GetNeededItems()
	{
		var neededItems = new List<NeededItem>();
		foreach (var objective in Objectives ?? new())
		{
			neededItems.AddRange(objective.GetNeededItems(Id));
		}

		foreach (var neededKey in NeededKeys ?? new())
		{
			neededItems.AddRange(neededKey.GetNeededItems(Id));
		}

		return neededItems;
	}
}
