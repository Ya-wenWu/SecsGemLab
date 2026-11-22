using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using NSubstitute;
using Secs4Net;
using SecsGemLab.Handshake;
using static Secs4Net.Item;

namespace SecsGemLab.Tests;

public class HsmsConnectionTests
{
    private static readonly TimeSpan Budget = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task Host_EstablishesCommunication_WithEquipment()
    {
        var identity = new EquipmentIdentity("MDLN", "SOFTREV");
        await using var link = await HsmsLink.StartAsync();

        var responder = new EquipmentResponder(link.Equipment, identity);
        var responderTask = responder.RunAsync(link.Token);

        var stopwatch = Stopwatch.StartNew();
        var confirmed = await new HostSession(link.Host, Budget).EstablishCommunicationsAsync(link.Token);
        stopwatch.Stop();

        Assert.Equal(identity, confirmed);
        Assert.Equal("MDLN", confirmed.Mdln);
        Assert.Equal("SOFTREV", confirmed.SoftRev);
        Assert.True(stopwatch.Elapsed < Budget, "Handshake must complete within 10 seconds.");

        await link.StopAsync();
        await responderTask;

        Assert.Equal(0, responder.UnhandledMessageCount);
    }

    [Fact]
    public async Task Equipment_CountsMessagesItDoesNotHandle()
    {
        await using var link = await HsmsLink.StartAsync();

        var responder = new EquipmentResponder(link.Equipment, new EquipmentIdentity("MDLN", "SOFTREV"));
        var responderTask = responder.RunAsync(link.Token);

        var areYouThere = new SecsMessage(s: 1, f: 1, replyExpected: false)
        {
            Name = "AreYouThere",
            SecsItem = L(),
        };
        await link.Host.SendAsync(areYouThere, link.Token);

        while (responder.UnhandledMessageCount == 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10), link.Token);
        }

        await link.StopAsync();
        await responderTask;

        Assert.Equal(1, responder.UnhandledMessageCount);
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    /// <summary>
    /// A connected Host (active) / Equipment (passive) pair on a loopback port chosen at run time.
    /// </summary>
    private sealed class HsmsLink : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new(Budget);
        private readonly HsmsConnection _hostConnection;
        private readonly HsmsConnection _equipmentConnection;

        private HsmsLink(
            HsmsConnection hostConnection,
            HsmsConnection equipmentConnection,
            SecsGem host,
            SecsGem equipment)
        {
            _hostConnection = hostConnection;
            _equipmentConnection = equipmentConnection;
            Host = host;
            Equipment = equipment;
        }

        public SecsGem Host { get; }

        public SecsGem Equipment { get; }

        public CancellationToken Token => _cts.Token;

        public static async Task<HsmsLink> StartAsync()
        {
            var port = GetFreeTcpPort();
            var activeOptions = Options.Create(new SecsGemOptions
            {
                IsActive = true,
                DeviceId = 0,
                IpAddress = "127.0.0.1",
                Port = port,
            });
            var passiveOptions = Options.Create(new SecsGemOptions
            {
                IsActive = false,
                DeviceId = 0,
                IpAddress = "127.0.0.1",
                Port = port,
            });

            var logger = Substitute.For<ISecsGemLogger>();
            var equipmentConnection = new HsmsConnection(passiveOptions, logger);
            var hostConnection = new HsmsConnection(activeOptions, logger);
            var link = new HsmsLink(
                hostConnection,
                equipmentConnection,
                new SecsGem(activeOptions, hostConnection, logger),
                new SecsGem(passiveOptions, equipmentConnection, logger));

            try
            {
                link._hostConnection.Start(link.Token);
                link._equipmentConnection.Start(link.Token);

                while (link._hostConnection.State is not ConnectionState.Selected ||
                       link._equipmentConnection.State is not ConnectionState.Selected)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10), link.Token);
                }
            }
            catch
            {
                await link.DisposeAsync();
                throw;
            }

            return link;
        }

        public async Task StopAsync() => await _cts.CancelAsync();

        public async ValueTask DisposeAsync()
        {
            await _cts.CancelAsync();
            Host.Dispose();
            Equipment.Dispose();
            await _hostConnection.DisposeAsync();
            await _equipmentConnection.DisposeAsync();
            _cts.Dispose();
        }
    }
}
