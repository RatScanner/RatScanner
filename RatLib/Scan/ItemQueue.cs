using System.Collections;
using RatLib.Scan;

public class ItemQueue : IEnumerable<ItemScan>
{
	private readonly Queue<ItemScan> queue = new();
	public event EventHandler Changed;

	protected virtual void OnChanged()
	{
		Changed?.Invoke(this, EventArgs.Empty);
		foreach (ItemScan itemScan in queue)
		{
			if (itemScan.DissapearAt != null && itemScan.DissapearAt < DateTimeOffset.Now.ToUnixTimeMilliseconds())
			{
				queue.Dequeue();
			}
			else
			{
				break;
			}
		}
	}
	public virtual void Enqueue(ItemScan item)
	{
		queue.Enqueue(item);
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
