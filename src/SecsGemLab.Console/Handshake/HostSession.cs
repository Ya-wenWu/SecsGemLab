using Secs4Net;

namespace SecsGemLab.Handshake;

/// <summary>
/// Host side (HSMS active) of the S1F13/S1F14 handshake.
/// </summary>
internal sealed class HostSession
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    private readonly SecsGem _secsGem;
    private readonly TimeSpan _timeout;

    public HostSession(SecsGem secsGem, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(secsGem);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);

        _secsGem = secsGem;
        _timeout = timeout;
    }

    public HostSession(SecsGem secsGem)
        : this(secsGem, DefaultTimeout)
    {
    }

    /// <summary>
    /// Sends S1F13 and returns the equipment identity carried by the S1F14 reply.
    /// </summary>
    /// <exception cref="TimeoutException">No reply arrived within the configured timeout.</exception>
    /// <exception cref="HandshakeException">The reply was missing or malformed.</exception>
    public async Task<EquipmentIdentity> EstablishCommunicationsAsync(CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(_timeout);

        try
        {
            using var request = HandshakeMessages.CreateRequest();
            using var reply = await _secsGem
                .SendAsync(request, timeoutSource.Token)
                .ConfigureAwait(false);

            return HandshakeMessages.ParseConfirm(reply);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Timed out waiting for S1F14 from the equipment.", ex);
        }
    }
}
