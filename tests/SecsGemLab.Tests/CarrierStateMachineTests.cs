using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class CarrierStateMachineTests
{
    private static CarrierStateMachine Verified()
    {
        var carrier = new CarrierStateMachine();
        Assert.True(carrier.OnIdRead());
        Assert.True(carrier.ProceedWithCarrier());
        Assert.True(carrier.OnSlotMapRead());
        Assert.True(carrier.ProceedWithCarrier());
        return carrier;
    }

    [Fact]
    public void FullFlow_ArrivalToComplete()
    {
        var carrier = new CarrierStateMachine();
        Assert.Equal(CarrierIdStatus.IdNotRead, carrier.IdStatus);

        Assert.True(carrier.OnIdRead());
        Assert.Equal(CarrierIdStatus.WaitingForHost, carrier.IdStatus);
        Assert.True(carrier.ProceedWithCarrier());
        Assert.Equal(CarrierIdStatus.IdVerificationOk, carrier.IdStatus);

        Assert.True(carrier.OnSlotMapRead());
        Assert.Equal(SlotMapStatus.WaitingForHost, carrier.SlotMapStatus);
        Assert.True(carrier.ProceedWithCarrier());
        Assert.Equal(SlotMapStatus.SlotMapVerificationOk, carrier.SlotMapStatus);

        Assert.True(carrier.StartAccess());
        Assert.Equal(CarrierAccessStatus.InAccess, carrier.AccessStatus);
        Assert.False(carrier.CanRelease);

        Assert.True(carrier.CompleteAccess());
        Assert.Equal(CarrierAccessStatus.CarrierComplete, carrier.AccessStatus);
        Assert.True(carrier.CanRelease);
    }

    [Fact]
    public void SlotMapCannotBeRead_BeforeTheIdIsVerified()
    {
        var carrier = new CarrierStateMachine();

        Assert.False(carrier.OnSlotMapRead());
        _ = carrier.OnIdRead();
        Assert.False(carrier.OnSlotMapRead());
        Assert.Equal(SlotMapStatus.SlotMapNotRead, carrier.SlotMapStatus);
    }

    [Fact]
    public void AccessCannotStart_WithoutBothVerifications()
    {
        var carrier = new CarrierStateMachine();
        Assert.False(carrier.StartAccess());

        _ = carrier.OnIdRead();
        _ = carrier.ProceedWithCarrier();
        Assert.False(carrier.StartAccess());
        Assert.Equal(CarrierAccessStatus.NotAccessed, carrier.AccessStatus);
    }

    [Fact]
    public void CancelCarrier_FailsTheIdVerification_AndAllowsRelease()
    {
        var carrier = new CarrierStateMachine();
        _ = carrier.OnIdRead();

        Assert.True(carrier.CancelCarrier());

        Assert.Equal(CarrierIdStatus.IdVerificationFailed, carrier.IdStatus);
        Assert.True(carrier.CanRelease);
        Assert.False(carrier.OnSlotMapRead());
    }

    [Fact]
    public void CancelCarrier_FailsTheSlotMapVerification()
    {
        var carrier = new CarrierStateMachine();
        _ = carrier.OnIdRead();
        _ = carrier.ProceedWithCarrier();
        _ = carrier.OnSlotMapRead();

        Assert.True(carrier.CancelCarrier());

        Assert.Equal(SlotMapStatus.SlotMapVerificationFailed, carrier.SlotMapStatus);
        Assert.True(carrier.CanRelease);
        Assert.False(carrier.StartAccess());
    }

    [Fact]
    public void ProceedAndCancel_DoNothing_WhenNothingIsPending()
    {
        var carrier = new CarrierStateMachine();

        Assert.False(carrier.ProceedWithCarrier());
        Assert.False(carrier.CancelCarrier());
        Assert.False(carrier.CanRelease);
    }

    [Fact]
    public void StopAccess_EndsInCarrierStopped()
    {
        var carrier = Verified();
        _ = carrier.StartAccess();

        Assert.True(carrier.StopAccess());

        Assert.Equal(CarrierAccessStatus.CarrierStopped, carrier.AccessStatus);
        Assert.True(carrier.CanRelease);
    }

    [Fact]
    public void AccessEnds_OnlyFromInAccess_AndOnlyOnce()
    {
        var carrier = Verified();

        Assert.False(carrier.CompleteAccess());
        Assert.False(carrier.StopAccess());

        _ = carrier.StartAccess();
        Assert.False(carrier.StartAccess());
        Assert.True(carrier.CompleteAccess());
        Assert.False(carrier.StopAccess());
    }

    [Fact]
    public void IdCanOnlyBeReadOnce()
    {
        var carrier = new CarrierStateMachine();
        _ = carrier.OnIdRead();

        Assert.False(carrier.OnIdRead());
    }
}
