using System;

namespace RatScanner.FetchModels;

[Serializable]
public class HideoutItem
{
	// Item data
	public string Id { get; set; }
	public int Needed { get; set; }
	public string StationId { get; set; } = "";

	public string ModuleId { get; set; } = "";

	// Id to cross reference with Tracking API
	public string HideoutPartId { get; set; } = "";
}
