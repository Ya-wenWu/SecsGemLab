namespace SecsGemLab.Gem;

/// <summary>DRACK values of S2F34.</summary>
internal enum DefineReportAck : byte
{
    Accepted = 0,
    OutOfSpace = 1,
    InvalidFormat = 2,
    AlreadyDefined = 3,
    VariableNotFound = 4,
}

/// <summary>LRACK values of S2F36.</summary>
internal enum LinkReportAck : byte
{
    Accepted = 0,
    OutOfSpace = 1,
    InvalidFormat = 2,
    AlreadyDefined = 3,
    EventNotFound = 4,
    ReportNotFound = 5,
}

/// <summary>ERACK values of S2F38.</summary>
internal enum EnableEventAck : byte
{
    Accepted = 0,
    EventNotFound = 1,
}

/// <summary>One report of S2F33: an ID and the variables it carries. No variables means delete.</summary>
internal sealed record ReportDefinition(uint ReportId, IReadOnlyList<uint> VariableIds);

/// <summary>One link of S2F35: an event and the reports sent with it. No reports means unlink.</summary>
internal sealed record EventLink(uint EventId, IReadOnlyList<uint> ReportIds);

/// <summary>
/// Dynamic event report configuration of SEMI E30: S2F33 (define report), S2F35 (link event report)
/// and S2F37 (enable or disable events). Every request is validated first and applied only if the
/// whole request is acceptable.
/// </summary>
/// <remarks>
/// Simplification: deleting a report also removes it from any event it was linked to.
/// </remarks>
internal sealed class ReportRegistry
{
    private readonly HashSet<uint> _knownVariables;
    private readonly HashSet<uint> _knownEvents;
    private readonly int _maxReports;
    private readonly Dictionary<uint, uint[]> _reports = [];
    private readonly Dictionary<uint, uint[]> _links = [];
    private readonly HashSet<uint> _enabled = [];

    public ReportRegistry(IEnumerable<uint> knownVariables, IEnumerable<uint> knownEvents, int maxReports = 256)
    {
        ArgumentNullException.ThrowIfNull(knownVariables);
        ArgumentNullException.ThrowIfNull(knownEvents);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxReports, 1);

        _knownVariables = [.. knownVariables];
        _knownEvents = [.. knownEvents];
        _maxReports = maxReports;
    }

    public int ReportCount => _reports.Count;

    /// <summary>S2F33. An empty request deletes every report and every link.</summary>
    public DefineReportAck Define(IReadOnlyList<ReportDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        if (definitions.Count == 0)
        {
            _reports.Clear();
            _links.Clear();
            return DefineReportAck.Accepted;
        }

        var seen = new HashSet<uint>();
        var added = 0;
        var removed = 0;
        foreach (var definition in definitions)
        {
            if (!seen.Add(definition.ReportId))
            {
                return DefineReportAck.InvalidFormat;
            }

            if (definition.VariableIds.Count == 0)
            {
                removed += _reports.ContainsKey(definition.ReportId) ? 1 : 0;
                continue;
            }

            if (_reports.ContainsKey(definition.ReportId))
            {
                return DefineReportAck.AlreadyDefined;
            }

            if (!definition.VariableIds.All(_knownVariables.Contains))
            {
                return DefineReportAck.VariableNotFound;
            }

            added++;
        }

        if (_reports.Count - removed + added > _maxReports)
        {
            return DefineReportAck.OutOfSpace;
        }

        foreach (var definition in definitions)
        {
            if (definition.VariableIds.Count == 0)
            {
                Remove(definition.ReportId);
            }
            else
            {
                _reports[definition.ReportId] = [.. definition.VariableIds];
            }
        }

        return DefineReportAck.Accepted;
    }

    /// <summary>S2F35. An empty request removes every link.</summary>
    public LinkReportAck Link(IReadOnlyList<EventLink> links)
    {
        ArgumentNullException.ThrowIfNull(links);

        if (links.Count == 0)
        {
            _links.Clear();
            return LinkReportAck.Accepted;
        }

        var seen = new HashSet<uint>();
        foreach (var link in links)
        {
            if (!seen.Add(link.EventId) || link.ReportIds.Distinct().Count() != link.ReportIds.Count)
            {
                return LinkReportAck.InvalidFormat;
            }

            if (!_knownEvents.Contains(link.EventId))
            {
                return LinkReportAck.EventNotFound;
            }

            if (link.ReportIds.Count == 0)
            {
                continue;
            }

            if (!link.ReportIds.All(_reports.ContainsKey))
            {
                return LinkReportAck.ReportNotFound;
            }

            if (_links.ContainsKey(link.EventId))
            {
                return LinkReportAck.AlreadyDefined;
            }
        }

        foreach (var link in links)
        {
            if (link.ReportIds.Count == 0)
            {
                _links.Remove(link.EventId);
            }
            else
            {
                _links[link.EventId] = [.. link.ReportIds];
            }
        }

        return LinkReportAck.Accepted;
    }

    /// <summary>S2F37. An empty event list applies to every known event.</summary>
    public EnableEventAck SetEnabled(bool enabled, IReadOnlyList<uint> eventIds)
    {
        ArgumentNullException.ThrowIfNull(eventIds);

        if (!eventIds.All(_knownEvents.Contains))
        {
            return EnableEventAck.EventNotFound;
        }

        foreach (var eventId in eventIds.Count == 0 ? _knownEvents.ToArray() : [.. eventIds])
        {
            _ = enabled ? _enabled.Add(eventId) : _enabled.Remove(eventId);
        }

        return EnableEventAck.Accepted;
    }

    public bool IsEnabled(uint eventId) => _enabled.Contains(eventId);

    /// <summary>
    /// The reports to attach to an S6F11 for <paramref name="eventId"/>. Empty when the event is
    /// disabled or has no linked reports, in which case no S6F11 carries report data.
    /// </summary>
    public IReadOnlyList<ReportDefinition> GetReportsFor(uint eventId)
    {
        if (!_enabled.Contains(eventId) || !_links.TryGetValue(eventId, out var reportIds))
        {
            return [];
        }

        return [.. reportIds.Select(id => new ReportDefinition(id, _reports[id]))];
    }

    private void Remove(uint reportId)
    {
        _ = _reports.Remove(reportId);
        foreach (var eventId in _links.Keys.ToArray())
        {
            var remaining = _links[eventId].Where(id => id != reportId).ToArray();
            if (remaining.Length == 0)
            {
                _ = _links.Remove(eventId);
            }
            else
            {
                _links[eventId] = remaining;
            }
        }
    }
}
