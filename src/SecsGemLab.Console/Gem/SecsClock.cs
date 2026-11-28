using System.Globalization;

namespace SecsGemLab.Gem;

/// <summary>TIME formats of S2F18 and S2F31.</summary>
internal enum ClockFormat
{
    /// <summary>YYMMDDhhmmss, 12 characters.</summary>
    TwelveDigit,

    /// <summary>YYYYMMDDhhmmsscc, 16 characters, centisecond resolution.</summary>
    SixteenDigit,
}

/// <summary>Formats and parses the TIME item. Times are handled as UTC.</summary>
internal static class SecsClock
{
    private const string Twelve = "yyMMddHHmmss";
    private const string Sixteen = "yyyyMMddHHmmss";

    public static string Format(DateTimeOffset time, ClockFormat format)
    {
        var utc = time.UtcDateTime;
        return format switch
        {
            ClockFormat.TwelveDigit => utc.ToString(Twelve, CultureInfo.InvariantCulture),
            ClockFormat.SixteenDigit => utc.ToString(Sixteen, CultureInfo.InvariantCulture)
                + (utc.Millisecond / 10).ToString("D2", CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown clock format."),
        };
    }

    /// <summary>
    /// Parses a 12 or 16 character TIME value, or returns null if it is malformed. Two-digit
    /// years are read as 2000 to 2099.
    /// </summary>
    public static DateTimeOffset? Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
        switch (text.Length)
        {
            case 12 when TryParse(text, Twelve, styles, out var twelve):
                return twelve;
            case 16 when TryParse(text[..14], Sixteen, styles, out var sixteen)
                && int.TryParse(text.AsSpan(14), NumberStyles.None, CultureInfo.InvariantCulture, out var centis):
                return sixteen.AddMilliseconds(centis * 10);
            default:
                return null;
        }
    }

    private static bool TryParse(string text, string format, DateTimeStyles styles, out DateTimeOffset result)
    {
        var ok = DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, styles, out var parsed);
        result = ok ? new DateTimeOffset(parsed, TimeSpan.Zero) : default;
        return ok;
    }
}
