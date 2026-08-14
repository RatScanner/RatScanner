using Newtonsoft.Json;
using System.Collections.Generic;

namespace RatScanner.TarkovDev.Json;

public class TarkovTask {
	[JsonProperty("id")]
	public string Id { get; set; } = string.Empty;

	[JsonProperty("name")]
	public string Name { get; set; } = string.Empty;

	[JsonProperty("trader")]
	public string Trader { get; set; } = string.Empty;

	[JsonProperty("wikiLink")]
	public string? WikiLink { get; set; }

	[JsonProperty("minPlayerLevel")]
	public int MinPlayerLevel { get; set; }

	[JsonProperty("taskRequirements")]
	public List<object> TaskRequirements { get; set; } = new();

	[JsonProperty("traderRequirements")]
	public List<object> TraderRequirements { get; set; } = new();

	[JsonProperty("objectives")]
	public List<TaskObjective> Objectives { get; set; } = new();

	[JsonProperty("failConditions")]
	public List<object> FailConditions { get; set; } = new();

	[JsonProperty("startRewards")]
	public object? StartRewards { get; set; }

	[JsonProperty("finishRewards")]
	public object? FinishRewards { get; set; }

	[JsonProperty("failureOutcome")]
	public object? FailureOutcome { get; set; }

	[JsonProperty("restartable")]
	public bool Restartable { get; set; }

	[JsonProperty("experience")]
	public int Experience { get; set; }

	[JsonProperty("factionName")]
	public string? FactionName { get; set; }

	[JsonProperty("neededKeys")]
	public object? NeededKeys { get; set; }

	[JsonProperty("availableDelaySecondsMin")]
	public int? AvailableDelaySecondsMin { get; set; }

	[JsonProperty("availableDelaySecondsMax")]
	public int? AvailableDelaySecondsMax { get; set; }

	[JsonProperty("otherRequirements")]
	public List<object> OtherRequirements { get; set; } = new();

	[JsonProperty("normalizedName")]
	public string NormalizedName { get; set; } = string.Empty;

	[JsonProperty("kappaRequired")]
	public bool? KappaRequired { get; set; }

	[JsonProperty("lightkeeperRequired")]
	public bool? LightkeeperRequired { get; set; }

	[JsonProperty("taskImageLink")]
	public string? TaskImageLink { get; set; }

	[JsonProperty("map")]
	public string? Map { get; set; }
}
