using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class SpoolBufferTests
{
    [Fact]
    public void Drain_ReturnsOldestFirst_AndEmptiesTheSpool()
    {
        var spool = new SpoolBuffer<int>(10, overwriteOldest: false);
        _ = spool.Enqueue(1);
        _ = spool.Enqueue(2);
        _ = spool.Enqueue(3);

        var batch = spool.Drain();

        Assert.Equal(new[] { 1, 2, 3 }, batch.ToArray());
        Assert.Equal(0, spool.ActualCount);
        Assert.Equal(3, spool.TotalCount);
    }

    [Fact]
    public void Drain_HonoursMaxTransmit()
    {
        var spool = new SpoolBuffer<int>(10, overwriteOldest: false);
        for (var i = 1; i <= 5; i++)
        {
            _ = spool.Enqueue(i);
        }

        var first = spool.Drain(2);
        var rest = spool.Drain();

        Assert.Equal(new[] { 1, 2 }, first.ToArray());
        Assert.Equal(new[] { 3, 4, 5 }, rest.ToArray());
    }

    [Fact]
    public void WhenFull_WithoutOverwrite_NewMessagesAreRefused()
    {
        var spool = new SpoolBuffer<int>(2, overwriteOldest: false);
        _ = spool.Enqueue(1);
        _ = spool.Enqueue(2);

        Assert.True(spool.IsFull);
        Assert.False(spool.Enqueue(3));
        Assert.Equal(new[] { 1, 2 }, spool.Drain().ToArray());
        Assert.Equal(2, spool.TotalCount);
    }

    [Fact]
    public void WhenFull_WithOverwrite_TheOldestIsReplaced()
    {
        var spool = new SpoolBuffer<int>(2, overwriteOldest: true);
        _ = spool.Enqueue(1);
        _ = spool.Enqueue(2);

        Assert.True(spool.Enqueue(3));

        Assert.Equal(new[] { 2, 3 }, spool.Drain().ToArray());
        Assert.Equal(3, spool.TotalCount);
    }

    [Fact]
    public void Purge_DiscardsEverythingAndResetsCounters()
    {
        var spool = new SpoolBuffer<int>(5, overwriteOldest: false);
        _ = spool.Enqueue(1);

        spool.Purge();

        Assert.Equal(0, spool.ActualCount);
        Assert.Equal(0, spool.TotalCount);
        Assert.Empty(spool.Drain());
    }

    [Fact]
    public void Constructor_AndDrain_RejectInvalidArguments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new SpoolBuffer<int>(0, overwriteOldest: false));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpoolBuffer<int>(1, overwriteOldest: false).Drain(-1));
    }
}
