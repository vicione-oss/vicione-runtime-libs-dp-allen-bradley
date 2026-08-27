namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;

/// <summary>
/// The outcome of one <see cref="ILogixTagAccess.ReadAsync"/>: the raw bytes it read, or why it failed.
/// </summary>
/// <remarks>
/// The result is a self-contained snapshot: the access is shared and synchronized per exchange, so
/// anything queried from it after the call returns could already belong to another caller's operation.
/// </remarks>
internal readonly record struct LogixTagReadResult
{
    private LogixTagReadResult(ReadOnlyMemory<byte> buffer, string? error)
    {
        Buffer = buffer;
        Error = error;
    }

    /// <summary>The raw tag bytes; empty when the read failed.</summary>
    public ReadOnlyMemory<byte> Buffer { get; }

    /// <summary>Why the read failed; <c>null</c> when it succeeded.</summary>
    public string? Error { get; }

    /// <summary>Whether the read succeeded.</summary>
    public bool Succeeded => Error is null;

    /// <summary>A read that succeeded, carrying the bytes it produced.</summary>
    public static LogixTagReadResult Ok(ReadOnlyMemory<byte> buffer) => new(buffer, null);

    /// <summary>A read that failed, carrying why.</summary>
    public static LogixTagReadResult Failed(string error) => new(default, error);
}
