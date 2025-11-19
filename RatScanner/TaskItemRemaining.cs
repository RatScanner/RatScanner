using RatScanner.TarkovDev.GraphQL;

namespace RatScanner;

public class TaskItemRemaining {
	public TaskItemRemaining(int itemCount, Task task) {
		ItemCount = itemCount;
		Task = task;
	}

	public int ItemCount { get; set; }
	public Task Task { get; set; }

}
