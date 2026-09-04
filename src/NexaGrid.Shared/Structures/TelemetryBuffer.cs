using System.Collections;

namespace NexaGrid.Shared.Structures;

/// <summary>
/// A bounded circular buffer optimised for recent telemetry values.
/// When full, the oldest reading is replaced by the newest reading.
/// </summary>
public sealed class TelemetryBuffer<T> : IReadOnlyCollection<T>
{
    private readonly T[] _items;
    private int _startIndex;
    private int _nextIndex;

    public TelemetryBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "The buffer capacity must be greater than zero.");
        }

        _items = new T[capacity];
    }

    public int Count { get; private set; }

    public int Capacity => _items.Length;

    public bool IsFull => Count == Capacity;

    public void Add(T item)
    {
        _items[_nextIndex] = item;

        _nextIndex = (_nextIndex + 1) % Capacity;

        if (Count < Capacity)
        {
            Count++;
        }
        else
        {
            _startIndex = (_startIndex + 1) % Capacity;
        }
    }

    public IReadOnlyList<T> ToList()
    {
        var result = new List<T>(Count);

        foreach (T item in this)
        {
            result.Add(item);
        }

        return result;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int index = 0; index < Count; index++)
        {
            int actualIndex =
                (_startIndex + index) % Capacity;

            yield return _items[actualIndex];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}