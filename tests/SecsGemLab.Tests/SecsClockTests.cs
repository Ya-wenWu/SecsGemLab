using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class SecsClockTests
{
    private static readonly DateTimeOffset Sample = new(2026, 11, 25, 8, 30, 15, 670, TimeSpan.Zero);

    [Fact]
    public void Format_TwelveDigit()
    {
        Assert.Equal("261125083015", SecsClock.Format(Sample, ClockFormat.TwelveDigit));
    }

    [Fact]
    public void Format_SixteenDigit_HasCentiseconds()
    {
        Assert.Equal("2026112508301567", SecsClock.Format(Sample, ClockFormat.SixteenDigit));
    }

    [Fact]
    public void Format_ConvertsToUtc()
    {
        var taipei = new DateTimeOffset(2026, 11, 25, 16, 30, 15, TimeSpan.FromHours(8));

        Assert.Equal("261125083015", SecsClock.Format(taipei, ClockFormat.TwelveDigit));
    }

    [Fact]
    public void Parse_RoundTripsBothFormats()
    {
        var twelve = SecsClock.Parse("261125083015");
        var sixteen = SecsClock.Parse("2026112508301567");

        Assert.Equal(new DateTimeOffset(2026, 11, 25, 8, 30, 15, TimeSpan.Zero), twelve);
        Assert.Equal(Sample, sixteen);
    }

    [Theory]
    [InlineData("")]
    [InlineData("2611250830")]
    [InlineData("26112508301")]
    [InlineData("261325083015")]
    [InlineData("2611250830AB")]
    [InlineData("2026112508301x67")]
    public void Parse_ReturnsNullForMalformedText(string text)
    {
        Assert.Null(SecsClock.Parse(text));
    }

    [Fact]
    public void Format_RejectsUnknownFormat()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SecsClock.Format(Sample, (ClockFormat)9));
    }
}
