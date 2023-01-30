using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class Quest
{
	[Obsolete("Use Task type instead.")]
	[JsonProperty("id")]
	public string Id { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("requirements")]
	public QuestRequirement Requirements { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("giver")]
	public Trader Giver { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("turnin")]
	public Trader Turnin { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("title")]
	public string Title { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("wikiLink")]
	public string WikiLink { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("exp")]
	public int Exp { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("unlocks")]
	public List<string> Unlocks { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("reputation")]
	public List<QuestRewardReputation> Reputation { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("objectives")]
	public List<QuestObjective> Objectives { get; set; }
}
