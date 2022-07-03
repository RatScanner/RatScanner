using System.Collections;
using System.Collections.Concurrent;
using RatLib.Scan;

public class ItemQueue : IEnumerable<ItemScan>
{
	private readonly ConcurrentQueue<ItemScan> queue = new();
	public event EventHandler Changed;

	protected virtual void OnChanged()
	{
		while (queue.Count > 1 && !(DateTimeOffset.Now.ToUnixTimeMilliseconds() > queue.First().DissapearAt))
		{
			if (!queue.TryDequeue(out _)) break;
		}
		Changed?.Invoke(this, EventArgs.Empty);
	}

	public virtual void Enqueue(ItemScan item)
	{
		queue.Enqueue(item);
		OnChanged();
	}

	public void EnqueueRange<T>(List<T> items) where T : ItemScan
	{
		items.ForEach(queue.Enqueue);
		OnChanged();
	}

	public int Count => queue.Count;

	IEnumerator IEnumerable.GetEnumerator() => queue.GetEnumerator();

	public IEnumerator<ItemScan> GetEnumerator() => queue.GetEnumerator();
}
