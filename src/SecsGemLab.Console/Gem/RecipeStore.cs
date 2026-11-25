namespace SecsGemLab.Gem;

/// <summary>PPGNT values of S7F2.</summary>
internal enum ProcessProgramGrant : byte
{
    Ok = 0,
    AlreadyHave = 1,
    NoSpace = 2,
    InvalidPpid = 3,
    Busy = 4,
}

/// <summary>ACKC7 values of S7F4 and S7F18.</summary>
internal enum ProcessProgramAck : byte
{
    Accepted = 0,
    PermissionNotGranted = 1,
    LengthError = 2,
    MatrixOverflow = 3,
    PpidNotFound = 4,
}

/// <summary>
/// Process program (recipe) storage of SEMI E30: S7F1 (inquire), S7F3 (download), S7F5 (upload),
/// S7F17 (delete) and S7F19 (list). Size limits keep a large recipe from exhausting memory.
/// </summary>
internal sealed class RecipeStore
{
    public const int MaxPpidLength = 80;

    private readonly SortedDictionary<string, byte[]> _recipes = new(StringComparer.Ordinal);
    private readonly long _maxTotalBytes;
    private readonly int _maxRecipeBytes;
    private long _totalBytes;

    public RecipeStore(long maxTotalBytes, int maxRecipeBytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxTotalBytes, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRecipeBytes, 1);

        _maxTotalBytes = maxTotalBytes;
        _maxRecipeBytes = maxRecipeBytes;
    }

    /// <summary>While true the equipment refuses new recipes.</summary>
    public bool IsBusy { get; set; }

    public long TotalBytes => _totalBytes;

    /// <summary>S7F1: may a recipe of this size be sent?</summary>
    public ProcessProgramGrant Inquire(string ppid, long length)
    {
        if (!IsValidPpid(ppid))
        {
            return ProcessProgramGrant.InvalidPpid;
        }

        if (IsBusy)
        {
            return ProcessProgramGrant.Busy;
        }

        if (_recipes.ContainsKey(ppid))
        {
            return ProcessProgramGrant.AlreadyHave;
        }

        return length is < 1 || length > _maxRecipeBytes || _totalBytes + length > _maxTotalBytes
            ? ProcessProgramGrant.NoSpace
            : ProcessProgramGrant.Ok;
    }

    /// <summary>S7F3. A recipe with the same PPID is replaced.</summary>
    public ProcessProgramAck Store(string ppid, ReadOnlySpan<byte> body)
    {
        if (!IsValidPpid(ppid) || IsBusy)
        {
            return ProcessProgramAck.PermissionNotGranted;
        }

        if (body.Length < 1 || body.Length > _maxRecipeBytes)
        {
            return ProcessProgramAck.LengthError;
        }

        var existing = _recipes.TryGetValue(ppid, out var old) ? old.Length : 0;
        if (_totalBytes - existing + body.Length > _maxTotalBytes)
        {
            return ProcessProgramAck.MatrixOverflow;
        }

        _recipes[ppid] = body.ToArray();
        _totalBytes += body.Length - existing;
        return ProcessProgramAck.Accepted;
    }

    /// <summary>S7F5: a copy of the recipe body, or null if the PPID is unknown.</summary>
    public byte[]? Get(string ppid) =>
        ppid is not null && _recipes.TryGetValue(ppid, out var body) ? (byte[])body.Clone() : null;

    /// <summary>S7F17. An empty list deletes every recipe. If any PPID is unknown nothing is deleted.</summary>
    public ProcessProgramAck Delete(IReadOnlyList<string> ppids)
    {
        ArgumentNullException.ThrowIfNull(ppids);

        if (ppids.Count == 0)
        {
            _recipes.Clear();
            _totalBytes = 0;
            return ProcessProgramAck.Accepted;
        }

        if (!ppids.All(p => p is not null && _recipes.ContainsKey(p)))
        {
            return ProcessProgramAck.PpidNotFound;
        }

        foreach (var ppid in ppids.Distinct(StringComparer.Ordinal))
        {
            _totalBytes -= _recipes[ppid].Length;
            _ = _recipes.Remove(ppid);
        }

        return ProcessProgramAck.Accepted;
    }

    /// <summary>S7F19.</summary>
    public IReadOnlyList<string> ListIds() => [.. _recipes.Keys];

    private static bool IsValidPpid(string? ppid) =>
        !string.IsNullOrWhiteSpace(ppid) && ppid.Length <= MaxPpidLength && ppid.All(char.IsAscii);
}
