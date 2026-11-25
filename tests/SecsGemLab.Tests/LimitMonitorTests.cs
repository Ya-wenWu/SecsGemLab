using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class LimitMonitorTests
{
    private static LimitMonitor Create()
    {
        var monitor = new LimitMonitor();
        Assert.True(monitor.Define(1, [new LimitDefinition(1, 10, 20)]));
        return monitor;
    }

    [Fact]
    public void FirstValue_OnlyRecordsTheZone()
    {
        var monitor = Create();

        Assert.Empty(monitor.Evaluate(1, 25));
    }

    [Fact]
    public void CrossingIntoAnotherZone_RaisesOneCrossing()
    {
        var monitor = Create();
        _ = monitor.Evaluate(1, 15);

        var crossings = monitor.Evaluate(1, 25);

        Assert.Single(crossings);
        Assert.Equal(LimitZone.Within, crossings[0].From);
        Assert.Equal(LimitZone.Above, crossings[0].To);
        Assert.Equal(25.0, crossings[0].Value);
        Assert.Equal(1u, crossings[0].VariableId);
    }

    [Fact]
    public void WobbleInsideTheDeadBand_RaisesNothing()
    {
        var monitor = Create();
        _ = monitor.Evaluate(1, 15);

        Assert.Empty(monitor.Evaluate(1, 19.9));
        Assert.Empty(monitor.Evaluate(1, 10.1));
        Assert.Empty(monitor.Evaluate(1, 20));
        Assert.Empty(monitor.Evaluate(1, 10));
    }

    [Fact]
    public void FallingBelow_IsReportedAndReturnIsReportedAgain()
    {
        var monitor = Create();
        _ = monitor.Evaluate(1, 15);

        var down = monitor.Evaluate(1, 5);
        var back = monitor.Evaluate(1, 12);

        Assert.Equal(LimitZone.Below, down[0].To);
        Assert.Equal(LimitZone.Below, back[0].From);
        Assert.Equal(LimitZone.Within, back[0].To);
    }

    [Fact]
    public void JumpAcrossTheWholeBand_IsOneCrossing()
    {
        var monitor = Create();
        _ = monitor.Evaluate(1, 5);

        var crossings = monitor.Evaluate(1, 30);

        Assert.Single(crossings);
        Assert.Equal(LimitZone.Below, crossings[0].From);
        Assert.Equal(LimitZone.Above, crossings[0].To);
    }

    [Fact]
    public void MultipleLimits_AreEvaluatedIndependently_InLimitIdOrder()
    {
        var monitor = new LimitMonitor();
        Assert.True(monitor.Define(1, [new LimitDefinition(2, 50, 60), new LimitDefinition(1, 10, 20)]));
        _ = monitor.Evaluate(1, 0);

        var crossings = monitor.Evaluate(1, 55);

        Assert.Equal(2, crossings.Count);
        Assert.Equal(1u, crossings[0].LimitId);
        Assert.Equal(2u, crossings[1].LimitId);
    }

    [Fact]
    public void Define_RejectsBadLimits_AndKeepsTheOldOnes()
    {
        var monitor = Create();

        Assert.False(monitor.Define(1, [new LimitDefinition(1, 30, 10)]));
        Assert.False(monitor.Define(1, [new LimitDefinition(1, 1, 2), new LimitDefinition(1, 3, 4)]));
        Assert.False(monitor.Define(1, [new LimitDefinition(1, double.NaN, 2)]));

        Assert.Single(monitor.Get(1));
        Assert.Equal(10.0, monitor.Get(1)[0].Lower);
    }

    [Fact]
    public void Define_WithEmptyList_RemovesTheLimits()
    {
        var monitor = Create();

        Assert.True(monitor.Define(1, []));

        Assert.Empty(monitor.Get(1));
        Assert.Empty(monitor.Evaluate(1, 100));
    }

    [Fact]
    public void UnknownVariable_RaisesNothing()
    {
        Assert.Empty(new LimitMonitor().Evaluate(7, 1));
    }
}
