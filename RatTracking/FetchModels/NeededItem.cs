namespace RatTracking.FetchModels;

[Serializable]
public class NeededItem
{
	// Initialization
	public NeededItem(string id)
	{
		Id = id;
	}

	// Item data
	public string Id { get; set; }

	// The quest number of this item needed by a player according to their progress
	public int QuestNeeded { get; set; } = 0;

	// The quest number of this item that this play has towards needed progress
	public int QuestHave { get; set; } = 0;

	// The hideout number of this item needed by a player according to their progress
	public int HideoutNeeded { get; set; } = 0;

	// The hideout number of this item that this play has towards needed progress
	public int HideoutHave { get; set; } = 0;

	// If any of the quest requirements outstanding are Found in raid required
	public bool FIR { get; set; } = false;

	// If this item is for ourselves, or someone else
	public bool Self { get; set; } = false;

	// Returns total remaining needed
	public int Remaining => QuestRemaining + HideoutRemaining;

	// Returns the number of outstanding items needed for quests
	public int QuestRemaining => QuestNeeded - QuestHave;

	// Returns the number of outstanding items needed for hideout
	public int HideoutRemaining => HideoutNeeded - HideoutHave;
}
