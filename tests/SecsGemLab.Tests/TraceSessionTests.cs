using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class TraceSessionTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 25, 0, 0, 0, TimeSpan.Zero);

    private static double Read(uint id) => id * 10.0;

    [Fact]
    public void Samples_AreGroupedByReportGroupSize()
    {
        var trace = new TraceSession(7, TimeSpan.FromSeconds(1), 6, 3, [1, 2]);

        Assert.Null(trace.AddSample(Read, Start));
        Assert.Null(trace.AddSample(Read, Start.AddSeconds(1)));
        var first = trace.AddSample(Read, Start.AddSeconds(2));

        Assert.NotNull(first);
        Assert.Equal(3, first.Count);
        Assert.Equal(1u, first[0].SampleNumber);
        Assert.Equal(3u, first[2].SampleNumber);
        Assert.Equal(7u, first[0].TraceId);
        Assert.Equal(new[] { 10.0, 20.0 }, first[0].Values.ToArray());
        Assert.False(trace.IsComplete);
    }

    [Fact]
    public void FinalPartialGroup_IsFlushedWhenTheTraceEnds()
    {
        var trace = new TraceSession(1, TimeSpan.FromSeconds(1), 4, 3, [1]);

        _ = trace.AddSample(Read, Start);
        _ = trace.AddSample(Read, Start);
        _ = trace.AddSample(Read, Start);
        _ = trace.AddSample(Read, Start);
        Assert.True(trace.IsComplete);
    }

    [Fact]
    public void LastBatch_ContainsTheRemainingSamples()
    {
        var trace = new TraceSession(1, TimeSpan.FromSeconds(1), 4, 3, [1]);
        _ = trace.AddSample(Read, Start);
        _ = trace.AddSample(Read, Start);
        _ = trace.AddSample(Read, Start);

        var last = trace.AddSample(Read, Start);

        Assert.NotNull(last);
        Assert.Single(last);
        Assert.Equal(4u, last[0].SampleNumber);
        Assert.True(trace.IsComplete);
    }

    [Fact]
    public void AfterCompletion_NoMoreSamplesAreTaken()
    {
        var trace = new TraceSession(1, TimeSpan.FromSeconds(1), 1, 1, [1]);
        Assert.NotNull(trace.AddSample(Read, Start));

        Assert.Null(trace.AddSample(Read, Start));
    }

    [Fact]
    public void Constructor_RejectsInvalidArguments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new TraceSession(1, TimeSpan.Zero, 1, 1, [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new TraceSession(1, TimeSpan.FromSeconds(1), 0, 1, [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new TraceSession(1, TimeSpan.FromSeconds(1), 1, 0, [1]));
        Assert.Throws<ArgumentException>(() => _ = new TraceSession(1, TimeSpan.FromSeconds(1), 1, 1, []));
    }
}
