namespace RatTracking.FetchModels;

[Serializable]
public class HideoutItem
{
	// Item data
	public string Id { get; set; }
	public int Needed { get; set; }
	public int StationId { get; set; }

	public int ModuleLevel { get; set; }

	// Id to cross reference with Tracking API
	public int HideoutObjectiveId { get; set; }
}
