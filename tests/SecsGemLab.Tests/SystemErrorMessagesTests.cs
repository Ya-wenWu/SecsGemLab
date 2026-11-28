using Secs4Net;
using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class SystemErrorMessagesTests
{
    private static readonly byte[] Header = [0, 1, 1, 13, 0, 0, 0, 0, 0, 7];

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(9)]
    [InlineData(11)]
    public void Create_BuildsStream9Message_WithABody(int expectedFunction)
    {
        var message = SystemErrorMessages.Create((SystemErrorFunction)expectedFunction, Header);

        Assert.Equal(9, message.S);
        Assert.Equal(expectedFunction, message.F);
        Assert.False(message.ReplyExpected);
        Assert.NotNull(message.SecsItem);
    }

    [Fact]
    public void Create_RejectsAHeaderThatIsNotTenBytes()
    {
        Assert.Throws<ArgumentException>(() => SystemErrorMessages.Create(
            SystemErrorFunction.IllegalData,
            new byte[9]));
    }

    [Fact]
    public void Create_RejectsAnUnknownFunction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SystemErrorMessages.Create(
            (SystemErrorFunction)99,
            Header));
    }

    [Fact]
    public void ForUnhandled_PicksS9F3OrS9F5()
    {
        Assert.Equal(SystemErrorFunction.UnrecognizedStreamType, SystemErrorMessages.ForUnhandled(streamKnown: false));
        Assert.Equal(SystemErrorFunction.UnrecognizedFunctionType, SystemErrorMessages.ForUnhandled(streamKnown: true));
    }
}
