namespace SecsGemLab.Handshake;

/// <summary>
/// Raised when the S1F13/S1F14 exchange fails or the reply is malformed.
/// </summary>
public sealed class HandshakeException : Exception
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
