namespace SecsGemLab.Gem;

/// <summary>One sample of a trace: SMPLN, STIME and the variable values.</summary>
internal sealed record TraceSample(uint TraceId, uint SampleNumber, DateTimeOffset Time, IReadOnlyList<double> Values);

/// <summary>
/// One trace of S2F23 (TRID, DSPER, TOTSMP, REPGSZ, SVIDs). The caller decides when to sample
/// (every <see cref="Period"/>) and sends each returned batch as one S6F1.
/// </summary>
internal sealed class TraceSession
{
    private readonly uint _totalSamples;
    private readonly uint _groupSize;
    private readonly List<TraceSample> _pending = [];
    private uint _taken;

    public TraceSession(
        uint traceId,
        TimeSpan period,
        uint totalSamples,
        uint reportGroupSize,
        IReadOnlyList<uint> variableIds)
    {
        ArgumentNullException.ThrowIfNull(variableIds);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(period, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfZero(totalSamples);
        ArgumentOutOfRangeException.ThrowIfZero(reportGroupSize);
        if (variableIds.Count == 0)
        {
            throw new ArgumentException("A trace needs at least one variable.", nameof(variableIds));
        }

        TraceId = traceId;
        Period = period;
        VariableIds = [.. variableIds];
        _totalSamples = totalSamples;
        _groupSize = reportGroupSize;
    }

    public uint TraceId { get; }

    public TimeSpan Period { get; }

    public IReadOnlyList<uint> VariableIds { get; }

    public bool IsComplete => _taken >= _totalSamples;

    /// <summary>
    /// Takes one sample. Returns the batch to send once <c>REPGSZ</c> samples are collected or the
    /// trace ends, and null while still collecting. After the trace is complete it returns null.
    /// </summary>
    public IReadOnlyList<TraceSample>? AddSample(Func<uint, double> read, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(read);

        if (IsComplete)
        {
            return null;
        }

        _taken++;
        _pending.Add(new TraceSample(TraceId, _taken, now, [.. VariableIds.Select(read)]));

        if (_pending.Count < _groupSize && !IsComplete)
        {
            return null;
        }

        var batch = _pending.ToArray();
        _pending.Clear();
        return batch;
    }
}
