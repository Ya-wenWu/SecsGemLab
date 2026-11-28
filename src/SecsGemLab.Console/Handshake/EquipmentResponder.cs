using Secs4Net;

namespace SecsGemLab.Handshake;

/// <summary>
/// Equipment side (HSMS passive): answers S1F13 with S1F14 and counts anything else it does not handle.
/// </summary>
internal sealed class EquipmentResponder
{
    private readonly SecsGem _secsGem;
    private readonly EquipmentIdentity _identity;
    private int _unhandledMessageCount;

    public EquipmentResponder(SecsGem secsGem, EquipmentIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(secsGem);
        ArgumentNullException.ThrowIfNull(identity);

        _secsGem = secsGem;
        _identity = identity;
    }

    public int UnhandledMessageCount => Volatile.Read(ref _unhandledMessageCount);

    /// <summary>
    /// Processes primary messages until <paramref name="cancellationToken"/> is cancelled.
    /// Cancellation is the normal way to stop and does not throw.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var primary in _secsGem.GetPrimaryMessageAsync(cancellationToken).ConfigureAwait(false))
            {
                if (HandshakeMessages.IsRequest(primary.PrimaryMessage))
                {
                    using var confirm = HandshakeMessages.CreateConfirm(_identity);
                    await primary.TryReplyAsync(confirm, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    Interlocked.Increment(ref _unhandledMessageCount);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected shutdown path.
        }
    }
}
