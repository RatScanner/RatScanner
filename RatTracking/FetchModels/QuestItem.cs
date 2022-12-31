namespace RatTracking.FetchModels;

[Serializable]
public class QuestItem
{
	// Item data
	public string Id { get; set; }
	public int Needed { get; set; }
	public string TaskId { get; set; }

	public bool FIR { get; set; }

	public bool HasAlternatives { get; set; } = false;

	// ID to cross reference with tracking
	public string TaskObjectiveId { get; set; }
}
