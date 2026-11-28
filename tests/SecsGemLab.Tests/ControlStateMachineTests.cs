using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class ControlStateMachineTests
{
    private static ControlStateMachine CreateOnline()
    {
        var machine = new ControlStateMachine();
        machine.OperatorGoOnline();
        machine.OnAttemptOnlineResult(hostReplied: true);
        return machine;
    }

    [Fact]
    public void StartsEquipmentOffline()
    {
        var machine = new ControlStateMachine();

        Assert.Equal(ControlState.EquipmentOffline, machine.State);
        Assert.False(machine.IsOnline);
        Assert.False(machine.IsRemoteCommandAllowed);
    }

    [Fact]
    public void OperatorOnline_WithHostReply_GoesToPreferredOnlineState()
    {
        var machine = CreateOnline();

        Assert.Equal(ControlState.OnlineRemote, machine.State);
        Assert.True(machine.IsRemoteCommandAllowed);
    }

    [Fact]
    public void OperatorOnline_WithoutHostReply_GoesHostOffline()
    {
        var machine = new ControlStateMachine();
        machine.OperatorGoOnline();

        Assert.Equal(ControlState.AttemptOnline, machine.State);

        machine.OnAttemptOnlineResult(hostReplied: false);

        Assert.Equal(ControlState.HostOffline, machine.State);
    }

    [Fact]
    public void PreferredLocal_IsHonoured()
    {
        var machine = new ControlStateMachine(ControlState.OnlineLocal);
        machine.OperatorGoOnline();
        machine.OnAttemptOnlineResult(hostReplied: true);

        Assert.Equal(ControlState.OnlineLocal, machine.State);
        Assert.False(machine.IsRemoteCommandAllowed);
    }

    [Fact]
    public void PreferredState_MustBeOnline()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new ControlStateMachine(ControlState.HostOffline));
    }

    [Fact]
    public void HostCannotForceEquipmentOffline_ToOnline()
    {
        var machine = new ControlStateMachine();

        Assert.Equal(OnlineAck.NotAllowed, machine.RequestOnline());
        Assert.Equal(ControlState.EquipmentOffline, machine.State);
    }

    [Fact]
    public void HostOfflineRoundTrip()
    {
        var machine = CreateOnline();

        Assert.Equal(ControlStateMachine.OffLineAcknowledged, machine.RequestOffline());
        Assert.Equal(ControlState.HostOffline, machine.State);

        Assert.Equal(OnlineAck.Accepted, machine.RequestOnline());
        Assert.Equal(ControlState.OnlineRemote, machine.State);
    }

    [Fact]
    public void RequestOnline_WhenAlreadyOnline_ReportsAlreadyOnline()
    {
        var machine = CreateOnline();

        Assert.Equal(OnlineAck.AlreadyOnline, machine.RequestOnline());
    }

    [Fact]
    public void RequestOffline_WhenEquipmentOffline_StillAcknowledges()
    {
        var machine = new ControlStateMachine();

        Assert.Equal(ControlStateMachine.OffLineAcknowledged, machine.RequestOffline());
        Assert.Equal(ControlState.EquipmentOffline, machine.State);
    }

    [Fact]
    public void OperatorSwitchesLocalRemote_OnlyWhileOnline()
    {
        var machine = new ControlStateMachine();
        Assert.False(machine.SwitchToLocal());

        machine = CreateOnline();
        Assert.True(machine.SwitchToLocal());
        Assert.Equal(ControlState.OnlineLocal, machine.State);
        Assert.True(machine.SwitchToRemote());
        Assert.Equal(ControlState.OnlineRemote, machine.State);
    }

    [Fact]
    public void OperatorOffline_WorksFromAnyState()
    {
        var machine = CreateOnline();

        machine.OperatorGoOffline();

        Assert.Equal(ControlState.EquipmentOffline, machine.State);
    }
}
