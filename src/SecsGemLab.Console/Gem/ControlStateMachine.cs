namespace SecsGemLab.Gem;

/// <summary>Control states of SEMI E30.</summary>
internal enum ControlState
{
    EquipmentOffline,
    AttemptOnline,
    HostOffline,
    OnlineLocal,
    OnlineRemote,
}

/// <summary>ONLACK values of S1F18.</summary>
internal enum OnlineAck : byte
{
    Accepted = 0,
    NotAllowed = 1,
    AlreadyOnline = 2,
}

/// <summary>
/// Control state model of SEMI E30. The operator decides whether the equipment may go online
/// (the host cannot override EQUIPMENT OFFLINE), while the host can take the equipment
/// to HOST OFFLINE and back.
/// </summary>
internal sealed class ControlStateMachine
{
    /// <summary>OFLACK value of S1F16. The equipment always acknowledges.</summary>
    public const byte OffLineAcknowledged = 0;

    private readonly ControlState _preferredOnline;

    public ControlStateMachine(ControlState preferredOnline = ControlState.OnlineRemote)
    {
        if (preferredOnline is not (ControlState.OnlineLocal or ControlState.OnlineRemote))
        {
            throw new ArgumentOutOfRangeException(
                nameof(preferredOnline),
                preferredOnline,
                "The preferred online state must be OnlineLocal or OnlineRemote.");
        }

        _preferredOnline = preferredOnline;
    }

    public ControlState State { get; private set; } = ControlState.EquipmentOffline;

    public bool IsOnline => State is ControlState.OnlineLocal or ControlState.OnlineRemote;

    /// <summary>Host commands (S2F41, S2F49 ...) are only accepted in ONLINE REMOTE.</summary>
    public bool IsRemoteCommandAllowed => State == ControlState.OnlineRemote;

    /// <summary>Operator selects ON-LINE. The equipment then tells the host with S1F1.</summary>
    public void OperatorGoOnline()
    {
        if (State == ControlState.EquipmentOffline)
        {
            State = ControlState.AttemptOnline;
        }
    }

    /// <summary>Result of the S1F1 sent while attempting to go online.</summary>
    public void OnAttemptOnlineResult(bool hostReplied)
    {
        if (State == ControlState.AttemptOnline)
        {
            State = hostReplied ? _preferredOnline : ControlState.HostOffline;
        }
    }

    /// <summary>Operator selects OFF-LINE. Valid from every state.</summary>
    public void OperatorGoOffline() => State = ControlState.EquipmentOffline;

    /// <summary>Operator switch to LOCAL. Only while online.</summary>
    public bool SwitchToLocal() => TrySwitchOnline(ControlState.OnlineLocal);

    /// <summary>Operator switch to REMOTE. Only while online.</summary>
    public bool SwitchToRemote() => TrySwitchOnline(ControlState.OnlineRemote);

    /// <summary>S1F17 (request ON-LINE) from the host.</summary>
    public OnlineAck RequestOnline()
    {
        switch (State)
        {
            case ControlState.HostOffline:
                State = _preferredOnline;
                return OnlineAck.Accepted;
            case ControlState.OnlineLocal:
            case ControlState.OnlineRemote:
                return OnlineAck.AlreadyOnline;
            default:
                return OnlineAck.NotAllowed;
        }
    }

    /// <summary>S1F15 (request OFF-LINE) from the host. Online equipment goes to HOST OFFLINE.</summary>
    public byte RequestOffline()
    {
        if (IsOnline)
        {
            State = ControlState.HostOffline;
        }

        return OffLineAcknowledged;
    }

    private bool TrySwitchOnline(ControlState target)
    {
        if (!IsOnline)
        {
            return false;
        }

        State = target;
        return true;
    }
}
