using System.Collections;
using RatLib.Scan;

public class ItemQueue : IEnumerable<ItemScan>
{
	private readonly Queue<ItemScan> queue = new();
	public event EventHandler Changed;

	protected virtual void OnChanged()
	{
		while (queue.Count > 1 && !(DateTimeOffset.Now.ToUnixTimeMilliseconds() > queue.First().DissapearAt))
		{
			queue.Dequeue();
		Console.WriteLine(queue.Count);
		}
		Changed?.Invoke(this, EventArgs.Empty);
	}

	public virtual void Enqueue(ItemScan item)
	{
		queue.Enqueue(item);
		OnChanged();
	}

	public void EnqueueRange<T>(List<T> items) where T: ItemScan
	{
		items.ForEach(queue.Enqueue);
		OnChanged();
	}

	public int Count => queue.Count;

	public virtual ItemScan Dequeue()
	{
		ItemScan item = queue.Dequeue();
		OnChanged();
		return item;
	}

	IEnumerator IEnumerable.GetEnumerator() => queue.GetEnumerator();

	public IEnumerator<ItemScan> GetEnumerator() => queue.GetEnumerator();
}
