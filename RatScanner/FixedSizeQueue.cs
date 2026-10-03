using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class FixedSizeQueue<T> : IEnumerable<T> {
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    private readonly object _lock = new object();
    private T? _last;
    private bool _hasLast;

    public int Size { get; }

    public FixedSizeQueue(int size) {
        Size = size;
    }

    public void Enqueue(T obj) {
        lock (_lock) {
            // Re-selecting what is already showing would fill the history with
            // duplicates and make back-navigation useless, so it is ignored.
            if (_hasLast && EqualityComparer<T>.Default.Equals(_last, obj)) return;

            _queue.Enqueue(obj);
            _last = obj;
            _hasLast = true;

            // Ensure we don't exceed the designated size limit
            while (_queue.Count > Size) {
                _queue.TryDequeue(out _);
            }
        }
    }

    public bool TryDequeue(out T result) {
        lock (_lock) {
            if (!_queue.TryDequeue(out result)) return false;

            // The dequeued entry was the tail whenever the queue is now empty, so
            // the duplicate check has to start over.
            if (_queue.Count == 0) _hasLast = false;
            else _last = _queue.Last();
            return true;
        }
    }

    /// <summary>Empties the queue, oldest entries first.</summary>
    public void Clear() {
        lock (_lock) {
            while (_queue.TryDequeue(out _)) { }
            _hasLast = false;
        }
    }

    public int Count => _queue.Count;

    /// <summary>
    /// Snapshot of the queue, oldest first. ConcurrentQueue already enumerates a
    /// stable view, so callers get a consistent read without locking.
    /// </summary>
    public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
