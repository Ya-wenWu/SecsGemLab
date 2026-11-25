using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class ProcessJobStateMachineTests
{
    private static ProcessJobStateMachine Processing(bool autoStart = false)
    {
        var job = new ProcessJobStateMachine(autoStart);
        Assert.True(job.BeginSetup());
        Assert.True(job.SetupComplete());
        if (!autoStart)
        {
            Assert.True(job.TryApply(ProcessJobCommand.Start));
        }

        return job;
    }

    [Fact]
    public void HappyPath_WithManualStart()
    {
        var job = new ProcessJobStateMachine(autoStart: false);
        Assert.Equal(ProcessJobState.Queued, job.State);

        Assert.True(job.BeginSetup());
        Assert.Equal(ProcessJobState.SettingUp, job.State);
        Assert.True(job.SetupComplete());
        Assert.Equal(ProcessJobState.WaitingForStart, job.State);
        Assert.True(job.TryApply(ProcessJobCommand.Start));
        Assert.Equal(ProcessJobState.Processing, job.State);
        Assert.True(job.ProcessingComplete());

        Assert.Equal(ProcessJobState.ProcessComplete, job.State);
        Assert.True(job.IsFinished);
    }

    [Fact]
    public void AutoStart_SkipsWaitingForStart()
    {
        var job = Processing(autoStart: true);

        Assert.Equal(ProcessJobState.Processing, job.State);
    }

    [Fact]
    public void StartIsRefused_BeforeSetupCompletes()
    {
        var job = new ProcessJobStateMachine(autoStart: false);

        Assert.False(job.TryApply(ProcessJobCommand.Start));
        _ = job.BeginSetup();
        Assert.False(job.TryApply(ProcessJobCommand.Start));
        Assert.Equal(ProcessJobState.SettingUp, job.State);
    }

    [Fact]
    public void PauseWhileProcessing_GoesThroughPausing_AndResumesToProcessing()
    {
        var job = Processing();

        Assert.True(job.TryApply(ProcessJobCommand.Pause));
        Assert.Equal(ProcessJobState.Pausing, job.State);
        Assert.False(job.TryApply(ProcessJobCommand.Resume));

        Assert.True(job.PauseComplete());
        Assert.Equal(ProcessJobState.Paused, job.State);

        Assert.True(job.TryApply(ProcessJobCommand.Resume));
        Assert.Equal(ProcessJobState.Processing, job.State);
    }

    [Fact]
    public void PauseBeforeProcessing_ResumesToThePreviousState()
    {
        var job = new ProcessJobStateMachine(autoStart: false);
        _ = job.BeginSetup();
        _ = job.SetupComplete();

        Assert.True(job.TryApply(ProcessJobCommand.Pause));
        Assert.Equal(ProcessJobState.Paused, job.State);
        Assert.True(job.TryApply(ProcessJobCommand.Resume));

        Assert.Equal(ProcessJobState.WaitingForStart, job.State);
    }

    [Fact]
    public void Stop_GoesThroughStopping()
    {
        var job = Processing();

        Assert.True(job.TryApply(ProcessJobCommand.Stop));
        Assert.Equal(ProcessJobState.Stopping, job.State);
        Assert.False(job.IsFinished);

        Assert.True(job.StopComplete());
        Assert.Equal(ProcessJobState.Stopped, job.State);
        Assert.True(job.IsFinished);
    }

    [Fact]
    public void Abort_WorksWhileStopping()
    {
        var job = Processing();
        _ = job.TryApply(ProcessJobCommand.Stop);

        Assert.True(job.TryApply(ProcessJobCommand.Abort));
        Assert.True(job.AbortComplete());

        Assert.Equal(ProcessJobState.Aborted, job.State);
    }

    [Fact]
    public void Cancel_OnlyWhileQueued()
    {
        var queued = new ProcessJobStateMachine(autoStart: false);
        Assert.True(queued.TryApply(ProcessJobCommand.Cancel));
        Assert.Equal(ProcessJobState.Canceled, queued.State);
        Assert.True(queued.IsFinished);

        var running = Processing();
        Assert.False(running.TryApply(ProcessJobCommand.Cancel));
    }

    [Fact]
    public void FinishedJobs_AcceptNothing()
    {
        var job = Processing();
        _ = job.ProcessingComplete();

        foreach (var command in Enum.GetValues<ProcessJobCommand>())
        {
            Assert.False(job.TryApply(command));
        }

        Assert.False(job.BeginSetup());
        Assert.Equal(ProcessJobState.ProcessComplete, job.State);
    }

    [Fact]
    public void CompletionEvents_AreIgnoredInTheWrongState()
    {
        var job = new ProcessJobStateMachine(autoStart: false);

        Assert.False(job.SetupComplete());
        Assert.False(job.ProcessingComplete());
        Assert.False(job.PauseComplete());
        Assert.False(job.StopComplete());
        Assert.False(job.AbortComplete());
        Assert.Equal(ProcessJobState.Queued, job.State);
    }

    [Fact]
    public void UnknownCommand_Throws()
    {
        var job = new ProcessJobStateMachine(autoStart: false);

        Assert.Throws<ArgumentOutOfRangeException>(() => job.TryApply((ProcessJobCommand)99));
    }
}
