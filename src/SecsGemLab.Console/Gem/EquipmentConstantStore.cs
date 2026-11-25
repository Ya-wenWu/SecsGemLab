namespace SecsGemLab.Gem;

/// <summary>EAC values of S2F16.</summary>
internal enum EquipmentConstantAck : byte
{
    Accepted = 0,
    NotFound = 1,
    Busy = 2,
    OutOfRange = 3,
}

/// <summary>An equipment constant definition (S2F29 reports the name, range and default).</summary>
internal sealed record EquipmentConstant(uint Id, string Name, string Units, double Min, double Max, double Default);

/// <summary>
/// Equipment constants (ECID) that the host can read (S2F13) and write (S2F15). A write is
/// all-or-nothing: if any constant is unknown or out of range, none is changed.
/// </summary>
internal sealed class EquipmentConstantStore
{
    private readonly SortedDictionary<uint, EquipmentConstant> _definitions = [];
    private readonly Dictionary<uint, double> _values = [];

    /// <summary>While true the equipment refuses writes (for example during processing).</summary>
    public bool IsBusy { get; set; }

    public IReadOnlyList<EquipmentConstant> Definitions => [.. _definitions.Values];

    public void Register(EquipmentConstant constant)
    {
        ArgumentNullException.ThrowIfNull(constant);

        if (constant.Min > constant.Max || constant.Default < constant.Min || constant.Default > constant.Max)
        {
            throw new ArgumentException("Default must lie within [Min, Max].", nameof(constant));
        }

        if (!_definitions.TryAdd(constant.Id, constant))
        {
            throw new ArgumentException("An equipment constant with this ECID already exists.", nameof(constant));
        }

        _values[constant.Id] = constant.Default;
    }

    /// <summary>Current value, or null when the ECID is unknown.</summary>
    public double? Get(uint id) => _values.TryGetValue(id, out var value) ? value : null;

    /// <summary>S2F15: applies every change or none.</summary>
    public EquipmentConstantAck Set(IReadOnlyList<KeyValuePair<uint, double>> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        if (IsBusy)
        {
            return EquipmentConstantAck.Busy;
        }

        foreach (var (id, value) in changes)
        {
            if (!_definitions.TryGetValue(id, out var definition))
            {
                return EquipmentConstantAck.NotFound;
            }

            if (double.IsNaN(value) || value < definition.Min || value > definition.Max)
            {
                return EquipmentConstantAck.OutOfRange;
            }
        }

        foreach (var (id, value) in changes)
        {
            _values[id] = value;
        }

        return EquipmentConstantAck.Accepted;
    }
}
