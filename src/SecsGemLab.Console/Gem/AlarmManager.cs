namespace SecsGemLab.Gem;

/// <summary>An alarm definition. Category uses the low 7 bits of ALCD.</summary>
internal sealed record AlarmDefinition(uint Id, string Text, byte Category);

/// <summary>The content of an S5F1: ALCD, ALID, ALTX.</summary>
internal sealed record AlarmReport(byte AlarmCode, uint Id, string Text);

/// <summary>A row of S5F6 / S5F8.</summary>
internal sealed record AlarmStatus(AlarmDefinition Definition, bool IsSet, bool IsEnabled);

/// <summary>
/// Alarm management of SEMI E30: set and clear alarms, enable or disable reporting (S5F3) and
/// list them (S5F5, S5F7). The top bit of ALCD is 1 when the alarm is set and 0 when it is cleared.
/// </summary>
internal sealed class AlarmManager
{
    public const byte SetBit = 0x80;

    /// <summary>ACKC5 values of S5F2 and S5F4.</summary>
    public const byte Accepted = 0;

    public const byte Error = 1;

    private readonly SortedDictionary<uint, AlarmState> _alarms = [];

    public void Register(AlarmDefinition definition, bool enabled = true)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (definition.Category >= SetBit)
        {
            throw new ArgumentException("Category must fit in 7 bits.", nameof(definition));
        }

        if (!_alarms.TryAdd(definition.Id, new AlarmState(definition, enabled)))
        {
            throw new ArgumentException("An alarm with this ALID already exists.", nameof(definition));
        }
    }

    public bool IsSet(uint id) => _alarms.TryGetValue(id, out var state) && state.IsSet;

    /// <summary>
    /// Sets an alarm. Returns the S5F1 content, or null when nothing has to be sent (already set,
    /// or reporting is disabled for this alarm).
    /// </summary>
    /// <exception cref="KeyNotFoundException">The ALID is not registered.</exception>
    public AlarmReport? Set(uint id) => Change(id, isSet: true);

    /// <summary>Clears an alarm. Same return rules as <see cref="Set"/>.</summary>
    /// <exception cref="KeyNotFoundException">The ALID is not registered.</exception>
    public AlarmReport? Clear(uint id) => Change(id, isSet: false);

    /// <summary>S5F3. Returns ACKC5.</summary>
    public byte SetEnabled(uint id, bool enabled)
    {
        if (!_alarms.TryGetValue(id, out var state))
        {
            return Error;
        }

        state.IsEnabled = enabled;
        return Accepted;
    }

    /// <summary>S5F5.</summary>
    public IReadOnlyList<AlarmStatus> ListAll() =>
        [.. _alarms.Values.Select(s => new AlarmStatus(s.Definition, s.IsSet, s.IsEnabled))];

    /// <summary>S5F7.</summary>
    public IReadOnlyList<AlarmStatus> ListEnabled() => [.. ListAll().Where(s => s.IsEnabled)];

    private AlarmReport? Change(uint id, bool isSet)
    {
        if (!_alarms.TryGetValue(id, out var state))
        {
            throw new KeyNotFoundException($"Alarm {id} is not registered.");
        }

        if (state.IsSet == isSet)
        {
            return null;
        }

        state.IsSet = isSet;
        if (!state.IsEnabled)
        {
            return null;
        }

        var code = (byte)(state.Definition.Category | (isSet ? SetBit : 0));
        return new AlarmReport(code, id, state.Definition.Text);
    }

    private sealed class AlarmState(AlarmDefinition definition, bool enabled)
    {
        public AlarmDefinition Definition { get; } = definition;

        public bool IsSet { get; set; }

        public bool IsEnabled { get; set; } = enabled;
    }
}
