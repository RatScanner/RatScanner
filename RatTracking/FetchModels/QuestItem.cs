namespace RatTracking.FetchModels;

[Serializable]
public class QuestItem
{
	// Item data
	public string Id { get; set; }
	public int Needed { get; set; }
	public int QuestId { get; set; }

	public bool FIR { get; set; }

	// ID to cross reference with tracking
	public int QuestObjectiveId { get; set; }
}
