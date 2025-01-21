using RatScanner.TarkovDev.GraphQL;

namespace RatScanner;

public class TaskItemRemaining
{
	public TaskItemRemaining(int itemCount, Task task)
	{
		ItemCount = itemCount;
		Task = task;
	}

	public int ItemCount { get; set; }
	public Task Task { get; set; }


	public string ToFloatingUiString()
	{
		return "[" + Task.MinPlayerLevel + "] " + Task.Trader.Name + "=>" + Task.Name + " " + ItemCount + "x";
	}
}
