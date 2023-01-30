using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class TaskRewards
{
	[JsonProperty("traderStanding")]
	public List<TraderStanding> TraderStanding { get; set; }

	[JsonProperty("items")]
	public List<ContainedItem> Items { get; set; }

	[JsonProperty("offerUnlock")]
	public List<OfferUnlock> OfferUnlock { get; set; }

	[JsonProperty("skillLevelReward")]
	public List<SkillLevel> SkillLevelReward { get; set; }

	[JsonProperty("traderUnlock")]
	public List<Trader> TraderUnlock { get; set; }

	[JsonProperty("craftUnlock")]
	public List<Craft> CraftUnlock { get; set; }
}
