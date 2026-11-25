using Secs4Net;
using static Secs4Net.Item;

namespace SecsGemLab.Gem;

/// <summary>Functions of Stream 9 whose body is the 10-byte header of the rejected message.</summary>
internal enum SystemErrorFunction
{
    UnrecognizedDeviceId = 1,
    UnrecognizedStreamType = 3,
    UnrecognizedFunctionType = 5,
    IllegalData = 7,
    TransactionTimerTimeout = 9,
    DataTooLong = 11,
}

/// <summary>
/// Builds S9Fx system error messages. S9F13 (conversation timeout) has a different body
/// and is not covered here.
/// </summary>
internal static class SystemErrorMessages
{
    public const int Stream = 9;

    /// <summary>A SECS-II message header is always 10 bytes.</summary>
    public const int HeaderLength = 10;

    public static SecsMessage Create(SystemErrorFunction function, ReadOnlySpan<byte> messageHeader)
    {
        if (!Enum.IsDefined(function))
        {
            throw new ArgumentOutOfRangeException(nameof(function), function, "Unknown S9 function.");
        }

        if (messageHeader.Length != HeaderLength)
        {
            throw new ArgumentException("The message header must be exactly 10 bytes.", nameof(messageHeader));
        }

        return new SecsMessage(s: Stream, f: (int)function, replyExpected: false)
        {
            Name = $"S9F{(int)function}",
            SecsItem = B(messageHeader.ToArray()),
        };
    }

    /// <summary>Picks S9F3 or S9F5 for an unhandled primary message.</summary>
    public static SystemErrorFunction ForUnhandled(bool streamKnown) =>
        streamKnown ? SystemErrorFunction.UnrecognizedFunctionType : SystemErrorFunction.UnrecognizedStreamType;
}
