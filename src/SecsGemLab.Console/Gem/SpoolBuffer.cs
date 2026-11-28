namespace SecsGemLab.Gem;

/// <summary>
/// Spool of SEMI E30: while communication is down the equipment keeps outgoing primary messages
/// and sends them later (S6F23 with RSDC 0) or throws them away (RSDC 1).
/// </summary>
/// <typeparam name="T">The spooled message type.</typeparam>
internal sealed class SpoolBuffer<T>
{
    private readonly Queue<T> _queue = new();
    private readonly int _maxSize;
    private readonly bool _overwriteOldest;

    /// <param name="maxSize">Largest number of messages kept.</param>
    /// <param name="overwriteOldest">
    /// When full: true replaces the oldest message, false refuses new ones
    /// (the OverWriteSpool equipment constant).
    /// </param>
    public SpoolBuffer(int maxSize, bool overwriteOldest)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSize, 1);

        _maxSize = maxSize;
        _overwriteOldest = overwriteOldest;
    }

    /// <summary>Messages currently held (SpoolCountActual).</summary>
    public int ActualCount => _queue.Count;

    /// <summary>Messages accepted since the last purge (SpoolCountTotal).</summary>
    public int TotalCount { get; private set; }

    public bool IsFull => _queue.Count >= _maxSize;

    /// <summary>Adds a message. Returns false when the spool is full and may not overwrite.</summary>
    public bool Enqueue(T message)
    {
        if (IsFull)
        {
            if (!_overwriteOldest)
            {
                return false;
            }

            _ = _queue.Dequeue();
        }

        _queue.Enqueue(message);
        TotalCount++;
        return true;
    }

    /// <summary>
    /// Removes and returns up to <paramref name="maxCount"/> messages, oldest first
    /// (MaxSpoolTransmit). A value of zero means all of them.
    /// </summary>
    public IReadOnlyList<T> Drain(int maxCount = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxCount);

        var count = maxCount == 0 ? _queue.Count : Math.Min(maxCount, _queue.Count);
        var batch = new List<T>(count);
        for (var i = 0; i < count; i++)
        {
            batch.Add(_queue.Dequeue());
        }

        return batch;
    }

    /// <summary>Discards everything and resets the counters (RSDC 1).</summary>
    public void Purge()
    {
        _queue.Clear();
        TotalCount = 0;
    }
}
