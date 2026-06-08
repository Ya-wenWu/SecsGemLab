using Microsoft.Extensions.Options;
using NSubstitute;
using Secs4Net;
using static Secs4Net.Item;

namespace SecsGemLab.Tests;

public class HsmsConnectionTests
{
    [Fact]
    public async Task Host_EstablishesCommunication_WithEquipment()
    {
        var activeOptions = Options.Create(new SecsGemOptions
        {
            IsActive = true,
            DeviceId = 0,
            IpAddress = "127.0.0.1",
            Port = 15000,
        });

        var passiveOptions = Options.Create(new SecsGemOptions
        {
            IsActive = false,
            DeviceId = 0,
            IpAddress = "127.0.0.1",
            Port = 15000,
        });

        var logger = Substitute.For<ISecsGemLogger>();

        await using var equipConnection = new HsmsConnection(passiveOptions, logger);
        await using var hostConnection = new HsmsConnection(activeOptions, logger);

        using var host = new SecsGem(activeOptions, hostConnection, logger);
        using var equipment = new SecsGem(passiveOptions, equipConnection, logger);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var token = cts.Token;

        hostConnection.Start(token);
        equipConnection.Start(token);

        SpinWait.SpinUntil(() =>
            hostConnection.State is ConnectionState.Selected &&
            equipConnection.State is ConnectionState.Selected);

        var s1f14 = new SecsMessage(s: 1, f: 14, replyExpected: false)
        {
            Name = "EstablishCommConfirm",
            SecsItem = L(A("MDLN"), A("SOFTREV")),
        };

        _ = Task.Run(async () =>
        {
            var primary = await equipment.GetPrimaryMessageAsync(token).FirstAsync(token);
            Assert.Equal(1, primary.PrimaryMessage.S);
            Assert.Equal(13, primary.PrimaryMessage.F);
            await primary.TryReplyAsync(s1f14);
        }, token);

        var ping = new SecsMessage(s: 1, f: 13)
        {
            Name = "EstablishCommRequest",
            SecsItem = L(),
        };

        var reply = await host.SendAsync(ping, token);

        Assert.NotNull(reply);
        Assert.Equal(1, reply.S);
        Assert.Equal(14, reply.F);
        Assert.NotNull(reply.SecsItem);
        var items = reply.SecsItem.Items.ToArray();
        Assert.Equal("MDLN", items[0].GetString());
        Assert.Equal("SOFTREV", items[1].GetString());
    }
}
