using System.Diagnostics.CodeAnalysis;

namespace SecsGemLab.Handshake;

/// <summary>
/// Raised when the S1F13/S1F14 exchange fails or the reply is malformed.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1064:Exceptions should be public",
    Justification = "The assembly is an executable, so CA1515 requires its types to be internal.")]
internal sealed class HandshakeException : Exception
{
    public HandshakeException()
    {
    }

    public HandshakeException(string message)
        : base(message)
    {
    }

    public HandshakeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
