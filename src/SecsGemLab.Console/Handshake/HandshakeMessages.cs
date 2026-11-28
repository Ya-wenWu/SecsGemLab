using Secs4Net;
using static Secs4Net.Item;

namespace SecsGemLab.Handshake;

/// <summary>
/// Builds and parses the S1F13 (Establish Communications Request) and
/// S1F14 (Establish Communications Confirm) messages used by this lab.
/// </summary>
/// <remarks>
/// The S1F14 body follows <c>specs/first-test</c>: a two-item list holding MDLN and SOFTREV.
/// SEMI E5 additionally prefixes a COMMACK item, which this lab does not model.
/// </remarks>
internal static class HandshakeMessages
{
    public const int Stream = 1;
    public const int RequestFunction = 13;
    public const int ConfirmFunction = 14;

    public static SecsMessage CreateRequest() =>
        new(s: Stream, f: RequestFunction)
        {
            Name = "EstablishCommRequest",
            SecsItem = L(),
        };

    public static SecsMessage CreateConfirm(EquipmentIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        return new SecsMessage(s: Stream, f: ConfirmFunction, replyExpected: false)
        {
            Name = "EstablishCommConfirm",
            SecsItem = L(A(identity.Mdln), A(identity.SoftRev)),
        };
    }

    public static bool IsRequest(SecsMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return message.S == Stream && message.F == RequestFunction;
    }

    public static EquipmentIdentity ParseConfirm(SecsMessage? message)
    {
        if (message is null)
        {
            throw new HandshakeException("No S1F14 reply was received.");
        }

        if (message.S != Stream || message.F != ConfirmFunction)
        {
            throw new HandshakeException("Expected S1F14 but received a different message.");
        }

        var items = message.SecsItem?.Items.ToArray() ?? [];
        if (items.Length != 2)
        {
            throw new HandshakeException("S1F14 must contain exactly two items: MDLN and SOFTREV.");
        }

        try
        {
            return new EquipmentIdentity(items[0].GetString(), items[1].GetString());
        }
        catch (ArgumentException ex)
        {
            throw new HandshakeException("S1F14 contains an invalid MDLN or SOFTREV value.", ex);
        }
    }
}
