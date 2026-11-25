using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class CommunicationStateMachineTests
{
    [Fact]
    public void StartsDisabled_AndEnableWaitsForCommAck()
    {
        var machine = new CommunicationStateMachine();

        Assert.Equal(CommunicationState.Disabled, machine.State);
        Assert.False(machine.IsEnabled);

        machine.Enable();

        Assert.Equal(CommunicationState.WaitCra, machine.State);
        Assert.True(machine.ShouldSendRequest);
    }

    [Fact]
    public void AcceptedCommAck_StartsCommunicating()
    {
        var machine = new CommunicationStateMachine();
        machine.Enable();

        machine.OnCommAck(0);

        Assert.True(machine.IsCommunicating);
        Assert.False(machine.ShouldSendRequest);
    }

    [Fact]
    public void RejectedCommAck_WaitsForDelayThenRetries()
    {
        var machine = new CommunicationStateMachine();
        machine.Enable();

        machine.OnCommAck(1);
        Assert.Equal(CommunicationState.WaitDelay, machine.State);

        machine.OnDelayElapsed();
        Assert.Equal(CommunicationState.WaitCra, machine.State);
    }

    [Fact]
    public void ReplyTimeout_MovesToWaitDelay()
    {
        var machine = new CommunicationStateMachine();
        machine.Enable();

        machine.OnReplyTimeout();

        Assert.Equal(CommunicationState.WaitDelay, machine.State);
    }

    [Fact]
    public void RemoteRequest_IsAcceptedWhileNotCommunicating()
    {
        var machine = new CommunicationStateMachine();
        machine.Enable();
        machine.OnReplyTimeout();

        Assert.True(machine.OnRemoteEstablishRequest());
        Assert.True(machine.IsCommunicating);
    }

    [Fact]
    public void RemoteRequest_IsIgnoredWhenDisabled()
    {
        var machine = new CommunicationStateMachine();

        Assert.False(machine.OnRemoteEstablishRequest());
        Assert.Equal(CommunicationState.Disabled, machine.State);
    }

    [Fact]
    public void ConnectionLost_ReturnsToWaitCra()
    {
        var machine = new CommunicationStateMachine();
        machine.Enable();
        machine.OnCommAck(0);

        machine.OnConnectionLost();

        Assert.Equal(CommunicationState.WaitCra, machine.State);
    }

    [Fact]
    public void Disable_WorksFromAnyState()
    {
        var machine = new CommunicationStateMachine();
        machine.Enable();
        machine.OnCommAck(0);

        machine.Disable();

        Assert.Equal(CommunicationState.Disabled, machine.State);
    }

    [Fact]
    public void CommAck_IsIgnoredOutsideWaitCra()
    {
        var machine = new CommunicationStateMachine();

        machine.OnCommAck(0);

        Assert.Equal(CommunicationState.Disabled, machine.State);
    }
}
