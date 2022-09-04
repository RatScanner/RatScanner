using System.Collections;
using System.Collections.Concurrent;
using RatLib.Scan;

public class ItemQueue : IEnumerable<ItemScan>
{
	private readonly ConcurrentQueue<ItemScan> _queue = new();
	public event EventHandler Changed;

	protected virtual void OnChanged()
	{
		while (_queue.Count > 1 && !(DateTimeOffset.Now.ToUnixTimeMilliseconds() > _queue.First().DissapearAt))
		{
			if (!_queue.TryDequeue(out _)) break;
		}
		Changed?.Invoke(this, EventArgs.Empty);
	}

	public virtual void Enqueue(ItemScan item)
	{
		_queue.Enqueue(item);
		OnChanged();
	}

	public void EnqueueRange<T>(List<T> items) where T : ItemScan
	{
		items.ForEach(_queue.Enqueue);
		OnChanged();
	}

	public int Count => _queue.Count;

	IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();

	public IEnumerator<ItemScan> GetEnumerator() => _queue.GetEnumerator();
}
