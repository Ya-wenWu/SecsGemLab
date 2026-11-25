namespace SecsGemLab.Gem;

/// <summary>Process job states of SEMI E40.</summary>
internal enum ProcessJobState
{
    Queued,
    SettingUp,
    WaitingForStart,
    Processing,
    ProcessComplete,
    Pausing,
    Paused,
    Stopping,
    Stopped,
    Aborting,
    Aborted,
    Canceled,
}

/// <summary>PRJobCommand values of S16F5.</summary>
internal enum ProcessJobCommand
{
    Start,
    Pause,
    Resume,
    Stop,
    Abort,
    Cancel,
}

/// <summary>
/// Process job state model of SEMI E40. Host commands go through <see cref="TryApply"/>; changes the
/// equipment makes by itself (finishing set-up, finishing the process, completing a pause, stop or
/// abort) have their own methods. Every method returns false, and changes nothing, when the move
/// is not allowed from the current state.
/// </summary>
internal sealed class ProcessJobStateMachine
{
    private readonly bool _autoStart;
    private ProcessJobState _resumeTarget = ProcessJobState.Processing;

    /// <param name="autoStart">
    /// PRProcessStart: when true the job moves on to PROCESSING as soon as set-up is complete
    /// instead of waiting for a Start command.
    /// </param>
    public ProcessJobStateMachine(bool autoStart)
    {
        _autoStart = autoStart;
    }

    public ProcessJobState State { get; private set; } = ProcessJobState.Queued;

    public bool IsFinished =>
        State is ProcessJobState.ProcessComplete or ProcessJobState.Stopped
            or ProcessJobState.Aborted or ProcessJobState.Canceled;

    /// <summary>Handles a host command (S16F5). Returns false when it is not valid now.</summary>
    public bool TryApply(ProcessJobCommand command) => command switch
    {
        ProcessJobCommand.Start => Move(ProcessJobState.WaitingForStart, ProcessJobState.Processing),
        ProcessJobCommand.Pause => Pause(),
        ProcessJobCommand.Resume => Resume(),
        ProcessJobCommand.Stop => Stop(),
        ProcessJobCommand.Abort => Abort(),
        ProcessJobCommand.Cancel => Move(ProcessJobState.Queued, ProcessJobState.Canceled),
        _ => throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown process job command."),
    };

    /// <summary>The equipment has the resources and starts setting up.</summary>
    public bool BeginSetup() => Move(ProcessJobState.Queued, ProcessJobState.SettingUp);

    /// <summary>Set-up finished. With auto start the job goes straight to PROCESSING.</summary>
    public bool SetupComplete() =>
        Move(ProcessJobState.SettingUp, _autoStart ? ProcessJobState.Processing : ProcessJobState.WaitingForStart);

    /// <summary>The last substrate is processed.</summary>
    public bool ProcessingComplete() => Move(ProcessJobState.Processing, ProcessJobState.ProcessComplete);

    /// <summary>The equipment reached a safe point and the pause is complete.</summary>
    public bool PauseComplete() => Move(ProcessJobState.Pausing, ProcessJobState.Paused);

    public bool StopComplete() => Move(ProcessJobState.Stopping, ProcessJobState.Stopped);

    public bool AbortComplete() => Move(ProcessJobState.Aborting, ProcessJobState.Aborted);

    private bool Pause()
    {
        switch (State)
        {
            case ProcessJobState.Processing:
                _resumeTarget = ProcessJobState.Processing;
                State = ProcessJobState.Pausing;
                return true;
            case ProcessJobState.SettingUp:
            case ProcessJobState.WaitingForStart:
                _resumeTarget = State;
                State = ProcessJobState.Paused;
                return true;
            default:
                return false;
        }
    }

    private bool Resume()
    {
        if (State != ProcessJobState.Paused)
        {
            return false;
        }

        State = _resumeTarget;
        return true;
    }

    private bool Stop()
    {
        if (State is ProcessJobState.SettingUp or ProcessJobState.WaitingForStart or ProcessJobState.Processing
            or ProcessJobState.Pausing or ProcessJobState.Paused)
        {
            State = ProcessJobState.Stopping;
            return true;
        }

        return false;
    }

    private bool Abort()
    {
        if (State is ProcessJobState.SettingUp or ProcessJobState.WaitingForStart or ProcessJobState.Processing
            or ProcessJobState.Pausing or ProcessJobState.Paused or ProcessJobState.Stopping)
        {
            State = ProcessJobState.Aborting;
            return true;
        }

        return false;
    }

    private bool Move(ProcessJobState from, ProcessJobState to)
    {
        if (State != from)
        {
            return false;
        }

        State = to;
        return true;
    }
}
