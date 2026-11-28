namespace SecsGemLab.Handshake;

/// <summary>
/// Equipment model name (MDLN) and software revision (SOFTREV) exchanged in S1F14.
/// </summary>
/// <remarks>
/// SEMI E5 limits both fields to 20 ASCII characters.
/// </remarks>
internal sealed record EquipmentIdentity
{
    public const int MaxLength = 20;

    public EquipmentIdentity(string mdln, string softRev)
    {
        Mdln = Validate(mdln, nameof(mdln));
        SoftRev = Validate(softRev, nameof(softRev));
    }

    public string Mdln { get; }

    public string SoftRev { get; }

    private static string Validate(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);

        if (value.Length > MaxLength)
        {
            throw new ArgumentException("Value must not exceed 20 characters (SEMI E5).", paramName);
        }

        if (!value.All(char.IsAscii))
        {
            throw new ArgumentException("Value must contain ASCII characters only.", paramName);
        }

        return value;
    }
}
