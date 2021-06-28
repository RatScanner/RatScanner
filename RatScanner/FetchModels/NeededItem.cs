using System;

namespace RatScanner.FetchModels
{
	[Serializable]
	public class NeededItem
	{
		// Item data
		public string Id { get; set; }
		// The quest number of this item needed by a player according to their progress
		public int QuestNeeded { get; set; }
		// The quest number of this item that this play has towards needed progress
		public int QuestHave { get; set; }
		// The hideout number of this item needed by a player according to their progress
		public int HideoutNeeded { get; set; }
		// The hideout number of this item that this play has towards needed progress
		public int HideoutHave { get; set; }
		// If any of the quest requirements outstanding are Found in raid required
		public int FIR { get; set; }

		// Returns total remaining needed
		public int Remaining
		{
			get
			{
				return QuestRemaining + HideoutRemaining;
			}
		}

		// Returns the number of outstanding items needed for quests
		public int QuestRemaining
		{
			get
			{
				return QuestNeeded - QuestHave;
			}
		}

		// Returns the number of outstanding items needed for hideout
		public int HideoutRemaining
		{
			get
			{
				return HideoutNeeded -  HideoutHave;
			}
		}
	}
}
