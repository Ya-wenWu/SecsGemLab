namespace SecsGemLab.Gem;

/// <summary>Control job states of SEMI E94.</summary>
internal enum ControlJobState
{
    Queued,
    Selected,
    WaitingForStart,
    Executing,
    Paused,
    Completed,
}

/// <summary>ControlJobCommand values of S16F27.</summary>
internal enum ControlJobCommand
{
    Start,
    Pause,
    Resume,
    Stop,
    Abort,
    Cancel,
}

/// <summary>How a control job ended. E94 uses COMPLETED for all of them.</summary>
internal enum ControlJobOutcome
{
    None,
    Completed,
    Stopped,
    Aborted,
    Canceled,
}

/// <summary>
/// Control job state model of SEMI E94. A control job runs one or more process jobs
/// (E40) and is done when all of them are finished.
/// </summary>
internal sealed class ControlJobStateMachine
{
    public ControlJobState State { get; private set; } = ControlJobState.Queued;

    public ControlJobOutcome Outcome { get; private set; }

    /// <summary>The job reached the front of the queue.</summary>
    public bool Select() => Move(ControlJobState.Queued, ControlJobState.Selected);

    /// <summary>The carriers and materials the job needs are available.</summary>
    public bool MaterialReady() => Move(ControlJobState.Selected, ControlJobState.WaitingForStart);

    /// <summary>Handles a host command (S16F27). Returns false when it is not valid now.</summary>
    public bool TryApply(ControlJobCommand command)
    {
        switch (command)
        {
            case ControlJobCommand.Start:
                return Move(ControlJobState.WaitingForStart, ControlJobState.Executing);
            case ControlJobCommand.Pause:
                return Move(ControlJobState.Executing, ControlJobState.Paused);
            case ControlJobCommand.Resume:
                return Move(ControlJobState.Paused, ControlJobState.Executing);
            case ControlJobCommand.Stop:
                return Finish(ControlJobOutcome.Stopped, ControlJobState.Selected, ControlJobState.WaitingForStart, ControlJobState.Executing, ControlJobState.Paused);
            case ControlJobCommand.Abort:
                return Finish(ControlJobOutcome.Aborted, ControlJobState.Selected, ControlJobState.WaitingForStart, ControlJobState.Executing, ControlJobState.Paused);
            case ControlJobCommand.Cancel:
                return Finish(ControlJobOutcome.Canceled, ControlJobState.Queued, ControlJobState.Selected, ControlJobState.WaitingForStart);
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown control job command.");
        }
    }

    /// <summary>
    /// Completes the job when every process job it owns is finished. Returns true only when this
    /// call ended the job. A control job with no process jobs never completes by itself.
    /// </summary>
    public bool CompleteWhenAllFinished(IReadOnlyCollection<ProcessJobStateMachine> processJobs)
    {
        ArgumentNullException.ThrowIfNull(processJobs);

        if (State != ControlJobState.Executing || processJobs.Count == 0 || !processJobs.All(j => j.IsFinished))
        {
            return false;
        }

        State = ControlJobState.Completed;
        Outcome = ControlJobOutcome.Completed;
        return true;
    }

    private bool Move(ControlJobState from, ControlJobState to)
    {
        if (State != from)
        {
            return false;
        }

        State = to;
        return true;
    }

    private bool Finish(ControlJobOutcome outcome, params ControlJobState[] allowedFrom)
    {
        if (!allowedFrom.Contains(State))
        {
            return false;
        }

        State = ControlJobState.Completed;
        Outcome = outcome;
        return true;
    }
}
