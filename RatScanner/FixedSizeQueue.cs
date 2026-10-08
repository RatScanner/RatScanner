using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A queue that keeps at most <see cref="Size"/> entries, dropping the oldest once
/// it is full. Adding something already in the queue moves it to the back rather
/// than storing a second copy.
/// </summary>
public class FixedSizeQueue<T> : IEnumerable<T> {
    private readonly LinkedList<T> _list = new();
    private readonly object _lock = new();

    public int Size { get; }

    public FixedSizeQueue(int size) {
        Size = size;
    }

    public void Enqueue(T obj) {
        lock (_lock) {
            // Pulled out before the new entry goes in, so picking something twice
            // leaves it in one place, at the back.
            var existing = _list.FindLast(obj);
            if (existing is not null) _list.Remove(existing);

            _list.AddLast(obj);

            while (_list.Count > Size) _list.RemoveFirst();
        }
    }

    public void Add(T obj) => Enqueue(obj);

    /// <summary>Takes the newest entry off the queue.</summary>
    public bool TryDequeue(out T result) {
        lock (_lock) {
            if (_list.Last is not { } last) {
                result = default!;
                return false;
            }

            _list.RemoveLast();
            result = last.Value;
            return true;
        }
    }

    /// <summary>Empties the queue, oldest entries first.</summary>
    public void Clear() {
        lock (_lock) _list.Clear();
    }

    public int Count => _list.Count;

    /// <summary>
    /// Snapshot of the queue, oldest first. The entries are copied out under the
    /// lock so a concurrent add cannot change the sequence mid-iteration.
    /// </summary>
    public IEnumerator<T> GetEnumerator() {
        lock (_lock) return ((IEnumerable<T>)_list.ToList()).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
