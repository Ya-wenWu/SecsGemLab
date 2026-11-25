using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class HsmsTimeoutsTests
{
    [Fact]
    public void Defaults_MatchTheUsualE37Values()
    {
        var timeouts = new HsmsTimeouts().Validated();

        Assert.Equal(TimeSpan.FromSeconds(45), timeouts.T3Reply);
        Assert.Equal(TimeSpan.FromSeconds(10), timeouts.T5ConnectSeparation);
        Assert.Equal(TimeSpan.FromSeconds(5), timeouts.T6ControlTransaction);
        Assert.Equal(TimeSpan.FromSeconds(10), timeouts.T7NotSelected);
        Assert.Equal(TimeSpan.FromSeconds(5), timeouts.T8NetworkIntercharacter);
    }

    [Fact]
    public void Validated_RejectsNonPositiveTimers()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HsmsTimeouts { T3Reply = TimeSpan.Zero }.Validated());
        Assert.Throws<ArgumentOutOfRangeException>(() => new HsmsTimeouts { T8NetworkIntercharacter = TimeSpan.FromSeconds(-1) }.Validated());
    }

    [Fact]
    public void WithExpression_ChangesOnlyTheNamedTimer()
    {
        var changed = new HsmsTimeouts() with { T3Reply = TimeSpan.FromSeconds(10) };

        Assert.Equal(TimeSpan.FromSeconds(10), changed.T3Reply);
        Assert.Equal(TimeSpan.FromSeconds(10), changed.T5ConnectSeparation);
    }
}
