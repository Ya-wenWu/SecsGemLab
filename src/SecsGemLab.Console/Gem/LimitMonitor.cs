namespace SecsGemLab.Gem;

/// <summary>Where a monitored value lies relative to one limit's dead band.</summary>
internal enum LimitZone
{
    Below,
    Within,
    Above,
}

/// <summary>One limit of S2F45: below Lower is BELOW, above Upper is ABOVE, between is the dead band.</summary>
internal sealed record LimitDefinition(uint LimitId, double Lower, double Upper);

/// <summary>A zone change that should raise an event.</summary>
internal sealed record LimitCrossing(uint VariableId, uint LimitId, LimitZone From, LimitZone To, double Value);

/// <summary>
/// Variable limit monitoring (S2F45/S2F47). A zone change raises one crossing. Because the
/// dead band sits between the two boundaries, a value that wobbles inside it does not raise events.
/// </summary>
/// <remarks>
/// Simplification: every zone change is reported. SEMI E30 lets the host choose which
/// transitions matter. Confirm the exact UPPERDB / LOWERDB semantics against the standard.
/// </remarks>
internal sealed class LimitMonitor
{
    private readonly Dictionary<uint, Dictionary<uint, LimitState>> _limits = [];

    /// <summary>
    /// S2F45 for one variable. An empty list removes all its limits. Returns false (VLAACK error)
    /// when a limit is inconsistent, in which case nothing changes.
    /// </summary>
    public bool Define(uint variableId, IReadOnlyList<LimitDefinition> limits)
    {
        ArgumentNullException.ThrowIfNull(limits);

        if (limits.Count == 0)
        {
            _ = _limits.Remove(variableId);
            return true;
        }

        var ids = new HashSet<uint>();
        foreach (var limit in limits)
        {
            if (double.IsNaN(limit.Lower) || double.IsNaN(limit.Upper) || limit.Lower > limit.Upper || !ids.Add(limit.LimitId))
            {
                return false;
            }
        }

        _limits[variableId] = limits.ToDictionary(l => l.LimitId, l => new LimitState(l));
        return true;
    }

    /// <summary>S2F47.</summary>
    public IReadOnlyList<LimitDefinition> Get(uint variableId) =>
        _limits.TryGetValue(variableId, out var limits)
            ? [.. limits.Values.Select(s => s.Definition).OrderBy(d => d.LimitId)]
            : [];

    /// <summary>
    /// Feeds a new value. The first value for a limit only records its zone. Later zone changes
    /// are returned in LimitId order.
    /// </summary>
    public IReadOnlyList<LimitCrossing> Evaluate(uint variableId, double value)
    {
        if (!_limits.TryGetValue(variableId, out var limits))
        {
            return [];
        }

        var crossings = new List<LimitCrossing>();
        foreach (var state in limits.Values.OrderBy(s => s.Definition.LimitId))
        {
            var zone = state.Definition.ZoneOf(value);
            if (state.Zone is { } previous && previous != zone)
            {
                crossings.Add(new LimitCrossing(variableId, state.Definition.LimitId, previous, zone, value));
            }

            state.Zone = zone;
        }

        return crossings;
    }

    private sealed class LimitState(LimitDefinition definition)
    {
        public LimitDefinition Definition { get; } = definition;

        public LimitZone? Zone { get; set; }
    }
}

internal static class LimitDefinitionExtensions
{
    public static LimitZone ZoneOf(this LimitDefinition definition, double value)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (value < definition.Lower)
        {
            return LimitZone.Below;
        }

        return value > definition.Upper ? LimitZone.Above : LimitZone.Within;
    }
}
