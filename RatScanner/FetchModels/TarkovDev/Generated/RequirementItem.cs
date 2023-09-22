using System.Collections.Generic;
using Newtonsoft.Json;

namespace RatScanner.FetchModels.TarkovDev;

public class RequirementItem
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("item")]
	public Item Item { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("quantity")]
	public int Quantity { get; set; }

	[JsonProperty("attributes")]
	public List<ItemAttribute> Attributes { get; set; }

	// Get needed items
	public List<NeededItem> GetNeededItems(string stationId, string moduleId)
	{
		return new List<NeededItem>
		{
			new NeededItem
			{
				Id = Item.Id,
				Count = Count,
				FoundInRaid = false,
				ProgressType = ProgressType.HideoutTurnin,
				ProgressId = Id,
				ModuleId = moduleId,
				StationId = stationId
			}
		};
	}
}
