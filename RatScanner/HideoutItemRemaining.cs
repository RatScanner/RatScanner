namespace RatScanner;

public class HideoutItemRemaining {

	public int ItemCount { get; set; }
	public string Name { get; set; }
	public int Level { get; set; }

	public HideoutItemRemaining(int itemCount, string name, int level) {
		ItemCount = itemCount;
		Name = name;
		Level = level;
	}

	public string ToFloatingUiString() {
		return "[" + Level + "] " + Name + " " + ItemCount + "x";
	}
}
