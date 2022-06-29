using System;
using System.Collections;
using RatLib.Scan;

public class ItemQueue : IEnumerable<ItemScan>
{
    private readonly Queue<ItemScan> queue = new Queue<ItemScan>();
    public event EventHandler Changed;
    protected virtual void OnChanged()
    {
        if (Changed != null) Changed(this, EventArgs.Empty);
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
    public int Count { get { return queue.Count; } }
	
    public virtual ItemScan Dequeue()
    {
        ItemScan item = queue.Dequeue();
        OnChanged();
        return item;
    }

	IEnumerator IEnumerable.GetEnumerator()
	{
		return queue.GetEnumerator();
	}

	public IEnumerator<ItemScan> GetEnumerator()
	{
		return queue.GetEnumerator();
	}
}
