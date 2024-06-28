using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class QuestRequirement
{
	[Obsolete("Use Task type instead.")]
	[JsonProperty("level")]
	public int? Level { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("quests")]
	public List<List<int?>> Quests { get; set; }

	[Obsolete("Use Task type instead.")]
	[JsonProperty("prerequisiteQuests")]
	public List<List<Quest>> PrerequisiteQuests { get; set; }
}
