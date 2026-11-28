using Secs4Net;
using SecsGemLab.Handshake;
using static Secs4Net.Item;

namespace SecsGemLab.Tests;

public class HandshakeMessagesTests
{
    [Fact]
    public void CreateRequest_IsS1F13_WithEmptyList()
    {
        var request = HandshakeMessages.CreateRequest();

        Assert.Equal(1, request.S);
        Assert.Equal(13, request.F);
        Assert.NotNull(request.SecsItem);
        Assert.Empty(request.SecsItem.Items.ToArray());
        Assert.True(HandshakeMessages.IsRequest(request));
    }

    [Fact]
    public void CreateConfirm_RoundTripsThroughParseConfirm()
    {
        var identity = new EquipmentIdentity("MDLN", "SOFTREV");

        var confirm = HandshakeMessages.CreateConfirm(identity);

        Assert.Equal(1, confirm.S);
        Assert.Equal(14, confirm.F);
        Assert.Equal(identity, HandshakeMessages.ParseConfirm(confirm));
    }

    [Fact]
    public void IsRequest_IsFalse_ForOtherMessages()
    {
        var areYouThere = new SecsMessage(s: 1, f: 1) { SecsItem = L() };

        Assert.False(HandshakeMessages.IsRequest(areYouThere));
    }

    [Fact]
    public void ParseConfirm_Throws_WhenReplyIsMissing()
    {
        Assert.Throws<HandshakeException>(() => HandshakeMessages.ParseConfirm(null));
    }

    [Fact]
    public void ParseConfirm_Throws_WhenReplyIsNotS1F14()
    {
        var wrong = new SecsMessage(s: 1, f: 13) { SecsItem = L(A("MDLN"), A("SOFTREV")) };

        Assert.Throws<HandshakeException>(() => HandshakeMessages.ParseConfirm(wrong));
    }

    [Fact]
    public void ParseConfirm_Throws_WhenItemCountIsNotTwo()
    {
        var tooFew = new SecsMessage(s: 1, f: 14, replyExpected: false) { SecsItem = L(A("MDLN")) };
        var tooMany = new SecsMessage(s: 1, f: 14, replyExpected: false)
        {
            SecsItem = L(A("MDLN"), A("SOFTREV"), A("EXTRA")),
        };

        Assert.Throws<HandshakeException>(() => HandshakeMessages.ParseConfirm(tooFew));
        Assert.Throws<HandshakeException>(() => HandshakeMessages.ParseConfirm(tooMany));
    }

    [Fact]
    public void ParseConfirm_Throws_WhenBodyIsEmpty()
    {
        var empty = new SecsMessage(s: 1, f: 14, replyExpected: false) { SecsItem = L() };

        Assert.Throws<HandshakeException>(() => HandshakeMessages.ParseConfirm(empty));
    }

    [Fact]
    public void ParseConfirm_Throws_WhenValueIsInvalid()
    {
        var tooLong = new SecsMessage(s: 1, f: 14, replyExpected: false)
        {
            SecsItem = L(A(new string('M', EquipmentIdentity.MaxLength + 1)), A("SOFTREV")),
        };

        Assert.Throws<HandshakeException>(() => HandshakeMessages.ParseConfirm(tooLong));
    }
}
