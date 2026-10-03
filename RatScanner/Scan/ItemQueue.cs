using RatScanner.Scan;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class ItemQueue : IEnumerable<ItemScan> {
	private readonly ConcurrentQueue<ItemScan> queue = new();
	public event EventHandler Changed;
	public event EventHandler<ItemScan>? Enqueued;

	protected virtual void OnChanged() {
		while (queue.Count > 1 && !(DateTimeOffset.Now.ToUnixTimeMilliseconds() > queue.First().DissapearAt)) {
			if (!queue.TryDequeue(out _)) break;
		}
		Changed?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnEnqueued(ItemScan item) {
		Enqueued?.Invoke(this, item);
	}

	public virtual void Enqueue(ItemScan item) {
		queue.Enqueue(item);
		OnEnqueued(item);
		OnChanged();
	}

	public void EnqueueRange<T>(List<T> items) where T : ItemScan {
		foreach (var item in items) Enqueue(item);
	}

	public int Count => queue.Count;

	IEnumerator IEnumerable.GetEnumerator() => queue.GetEnumerator();

	public IEnumerator<ItemScan> GetEnumerator() => queue.GetEnumerator();
}
