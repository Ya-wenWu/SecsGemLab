namespace SecsGemLab.Gem;

/// <summary>Communication states of SEMI E30 (NOT COMMUNICATING is split into its two substates).</summary>
internal enum CommunicationState
{
    Disabled,
    WaitCra,
    WaitDelay,
    Communicating,
}

/// <summary>
/// Communication state model of SEMI E30. WAIT CRA means an S1F13 was sent and the
/// S1F14 (communication request acknowledge) is awaited; WAIT DELAY means the equipment
/// is waiting for the establish-communications timer before trying again.
/// </summary>
internal sealed class CommunicationStateMachine
{
    public CommunicationState State { get; private set; } = CommunicationState.Disabled;

    public bool IsEnabled => State != CommunicationState.Disabled;

    public bool IsCommunicating => State == CommunicationState.Communicating;

    /// <summary>True when an S1F13 should be sent now.</summary>
    public bool ShouldSendRequest => State == CommunicationState.WaitCra;

    /// <summary>DISABLED to ENABLED. Has no effect when already enabled.</summary>
    public void Enable()
    {
        if (State == CommunicationState.Disabled)
        {
            State = CommunicationState.WaitCra;
        }
    }

    /// <summary>Any state to DISABLED.</summary>
    public void Disable() => State = CommunicationState.Disabled;

    /// <summary>S1F14 received. COMMACK 0 means accepted; anything else starts the retry delay.</summary>
    public void OnCommAck(byte commAck)
    {
        if (State != CommunicationState.WaitCra)
        {
            return;
        }

        State = commAck == 0 ? CommunicationState.Communicating : CommunicationState.WaitDelay;
    }

    /// <summary>No S1F14 arrived before the establish-communications timeout.</summary>
    public void OnReplyTimeout()
    {
        if (State == CommunicationState.WaitCra)
        {
            State = CommunicationState.WaitDelay;
        }
    }

    /// <summary>The retry delay elapsed. The next step is to send S1F13 again.</summary>
    public void OnDelayElapsed()
    {
        if (State == CommunicationState.WaitDelay)
        {
            State = CommunicationState.WaitCra;
        }
    }

    /// <summary>
    /// The remote side sent S1F13. While not communicating, the request is accepted and the
    /// caller must answer with S1F14 COMMACK 0. Returns false when the request was ignored.
    /// </summary>
    public bool OnRemoteEstablishRequest()
    {
        if (State is CommunicationState.WaitCra or CommunicationState.WaitDelay)
        {
            State = CommunicationState.Communicating;
            return true;
        }

        return State == CommunicationState.Communicating;
    }

    /// <summary>The HSMS connection dropped or was deselected.</summary>
    public void OnConnectionLost()
    {
        if (State == CommunicationState.Communicating)
        {
            State = CommunicationState.WaitCra;
        }
    }
}
