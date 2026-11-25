namespace SecsGemLab.Gem;

/// <summary>Carrier ID status of SEMI E87.</summary>
internal enum CarrierIdStatus
{
    IdNotRead,
    WaitingForHost,
    IdVerificationOk,
    IdVerificationFailed,
}

/// <summary>Slot map status of SEMI E87.</summary>
internal enum SlotMapStatus
{
    SlotMapNotRead,
    WaitingForHost,
    SlotMapVerificationOk,
    SlotMapVerificationFailed,
}

/// <summary>Carrier access status of SEMI E87.</summary>
internal enum CarrierAccessStatus
{
    NotAccessed,
    InAccess,
    CarrierComplete,
    CarrierStopped,
}

/// <summary>
/// Carrier object state models of SEMI E87 for one carrier: ID verification, slot map verification
/// and access. The host answers each verification with ProceedWithCarrier or CancelCarrier
/// (S3F17). Every method returns false, and changes nothing, when it is not valid now.
/// </summary>
/// <remarks>The load port transfer state and reservation model are not covered.</remarks>
internal sealed class CarrierStateMachine
{
    public CarrierIdStatus IdStatus { get; private set; } = CarrierIdStatus.IdNotRead;

    public SlotMapStatus SlotMapStatus { get; private set; } = SlotMapStatus.SlotMapNotRead;

    public CarrierAccessStatus AccessStatus { get; private set; } = CarrierAccessStatus.NotAccessed;

    /// <summary>The carrier may be taken away: it is done, stopped, or the host rejected it.</summary>
    public bool CanRelease =>
        AccessStatus is CarrierAccessStatus.CarrierComplete or CarrierAccessStatus.CarrierStopped
        || IdStatus == CarrierIdStatus.IdVerificationFailed
        || SlotMapStatus == SlotMapStatus.SlotMapVerificationFailed;

    /// <summary>The equipment read the carrier ID and asks the host to verify it.</summary>
    public bool OnIdRead()
    {
        if (IdStatus != CarrierIdStatus.IdNotRead)
        {
            return false;
        }

        IdStatus = CarrierIdStatus.WaitingForHost;
        return true;
    }

    /// <summary>The equipment read the slot map and asks the host to verify it. Needs a verified ID.</summary>
    public bool OnSlotMapRead()
    {
        if (IdStatus != CarrierIdStatus.IdVerificationOk || SlotMapStatus != SlotMapStatus.SlotMapNotRead)
        {
            return false;
        }

        SlotMapStatus = SlotMapStatus.WaitingForHost;
        return true;
    }

    /// <summary>Host accepts whichever verification is pending.</summary>
    public bool ProceedWithCarrier()
    {
        if (IdStatus == CarrierIdStatus.WaitingForHost)
        {
            IdStatus = CarrierIdStatus.IdVerificationOk;
            return true;
        }

        if (SlotMapStatus == SlotMapStatus.WaitingForHost)
        {
            SlotMapStatus = SlotMapStatus.SlotMapVerificationOk;
            return true;
        }

        return false;
    }

    /// <summary>Host rejects whichever verification is pending.</summary>
    public bool CancelCarrier()
    {
        if (IdStatus == CarrierIdStatus.WaitingForHost)
        {
            IdStatus = CarrierIdStatus.IdVerificationFailed;
            return true;
        }

        if (SlotMapStatus == SlotMapStatus.WaitingForHost)
        {
            SlotMapStatus = SlotMapStatus.SlotMapVerificationFailed;
            return true;
        }

        return false;
    }

    /// <summary>Processing of the carrier's substrates begins. Needs both verifications.</summary>
    public bool StartAccess()
    {
        if (IdStatus != CarrierIdStatus.IdVerificationOk
            || SlotMapStatus != SlotMapStatus.SlotMapVerificationOk
            || AccessStatus != CarrierAccessStatus.NotAccessed)
        {
            return false;
        }

        AccessStatus = CarrierAccessStatus.InAccess;
        return true;
    }

    public bool CompleteAccess() => EndAccess(CarrierAccessStatus.CarrierComplete);

    public bool StopAccess() => EndAccess(CarrierAccessStatus.CarrierStopped);

    private bool EndAccess(CarrierAccessStatus result)
    {
        if (AccessStatus != CarrierAccessStatus.InAccess)
        {
            return false;
        }

        AccessStatus = result;
        return true;
    }
}
