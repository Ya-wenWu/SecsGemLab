using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class ControlJobStateMachineTests
{
    private static ControlJobStateMachine Executing()
    {
        var job = new ControlJobStateMachine();
        Assert.True(job.Select());
        Assert.True(job.MaterialReady());
        Assert.True(job.TryApply(ControlJobCommand.Start));
        return job;
    }

    private static ProcessJobStateMachine FinishedProcessJob()
    {
        var pj = new ProcessJobStateMachine(autoStart: true);
        _ = pj.BeginSetup();
        _ = pj.SetupComplete();
        _ = pj.ProcessingComplete();
        return pj;
    }

    [Fact]
    public void HappyPath_EndsCompleted_WhenAllProcessJobsFinish()
    {
        var job = Executing();
        var running = new ProcessJobStateMachine(autoStart: true);
        _ = running.BeginSetup();
        _ = running.SetupComplete();
        var done = FinishedProcessJob();

        Assert.False(job.CompleteWhenAllFinished([done, running]));
        Assert.Equal(ControlJobState.Executing, job.State);

        _ = running.ProcessingComplete();

        Assert.True(job.CompleteWhenAllFinished([done, running]));
        Assert.Equal(ControlJobState.Completed, job.State);
        Assert.Equal(ControlJobOutcome.Completed, job.Outcome);
    }

    [Fact]
    public void ControlJobWithoutProcessJobs_NeverCompletesByItself()
    {
        var job = Executing();

        Assert.False(job.CompleteWhenAllFinished([]));
        Assert.Equal(ControlJobState.Executing, job.State);
    }

    [Fact]
    public void CompleteWhenAllFinished_IsIgnoredUnlessExecuting()
    {
        var job = new ControlJobStateMachine();

        Assert.False(job.CompleteWhenAllFinished([FinishedProcessJob()]));
    }

    [Fact]
    public void StartIsRefused_UntilMaterialIsReady()
    {
        var job = new ControlJobStateMachine();

        Assert.False(job.TryApply(ControlJobCommand.Start));
        _ = job.Select();
        Assert.False(job.TryApply(ControlJobCommand.Start));
        Assert.Equal(ControlJobState.Selected, job.State);
    }

    [Fact]
    public void PauseAndResume()
    {
        var job = Executing();

        Assert.True(job.TryApply(ControlJobCommand.Pause));
        Assert.Equal(ControlJobState.Paused, job.State);
        Assert.False(job.TryApply(ControlJobCommand.Pause));
        Assert.True(job.TryApply(ControlJobCommand.Resume));
        Assert.Equal(ControlJobState.Executing, job.State);
    }

    [Fact]
    public void StopAndAbort_EndInCompleted_WithTheirOutcome()
    {
        var stopped = Executing();
        Assert.True(stopped.TryApply(ControlJobCommand.Stop));
        Assert.Equal(ControlJobState.Completed, stopped.State);
        Assert.Equal(ControlJobOutcome.Stopped, stopped.Outcome);

        var aborted = Executing();
        _ = aborted.TryApply(ControlJobCommand.Pause);
        Assert.True(aborted.TryApply(ControlJobCommand.Abort));
        Assert.Equal(ControlJobOutcome.Aborted, aborted.Outcome);
    }

    [Fact]
    public void Cancel_IsAllowedBeforeExecution_ButNotAfter()
    {
        var queued = new ControlJobStateMachine();
        Assert.True(queued.TryApply(ControlJobCommand.Cancel));
        Assert.Equal(ControlJobOutcome.Canceled, queued.Outcome);

        var executing = Executing();
        Assert.False(executing.TryApply(ControlJobCommand.Cancel));
        Assert.Equal(ControlJobState.Executing, executing.State);
    }

    [Fact]
    public void CompletedJobs_AcceptNothing()
    {
        var job = Executing();
        _ = job.TryApply(ControlJobCommand.Stop);

        foreach (var command in Enum.GetValues<ControlJobCommand>())
        {
            Assert.False(job.TryApply(command));
        }

        Assert.False(job.Select());
        Assert.False(job.MaterialReady());
    }

    [Fact]
    public void UnknownCommand_Throws()
    {
        var job = new ControlJobStateMachine();

        Assert.Throws<ArgumentOutOfRangeException>(() => job.TryApply((ControlJobCommand)99));
    }
}
