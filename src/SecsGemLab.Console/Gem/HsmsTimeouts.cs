namespace SecsGemLab.Gem;

/// <summary>
/// HSMS timer parameters (SEMI E37). Defaults follow the values commonly used by equipment.
/// </summary>
internal sealed record HsmsTimeouts
{
    /// <summary>Reply timeout. A missing reply is reported with S9F9.</summary>
    public TimeSpan T3Reply { get; init; } = TimeSpan.FromSeconds(45);

    /// <summary>Minimum wait before an active entity tries to connect again.</summary>
    public TimeSpan T5ConnectSeparation { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>Control transaction timeout (Select, Deselect, Linktest).</summary>
    public TimeSpan T6ControlTransaction { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>How long a connection may stay TCP-connected without being selected.</summary>
    public TimeSpan T7NotSelected { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>Maximum gap between bytes of one message.</summary>
    public TimeSpan T8NetworkIntercharacter { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>Returns this instance after checking that every timer is positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A timer is zero or negative.</exception>
    public HsmsTimeouts Validated()
    {
        Require(T3Reply, nameof(T3Reply));
        Require(T5ConnectSeparation, nameof(T5ConnectSeparation));
        Require(T6ControlTransaction, nameof(T6ControlTransaction));
        Require(T7NotSelected, nameof(T7NotSelected));
        Require(T8NetworkIntercharacter, nameof(T8NetworkIntercharacter));
        return this;
    }

    private static void Require(TimeSpan value, string name)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero, name);
    }
}
