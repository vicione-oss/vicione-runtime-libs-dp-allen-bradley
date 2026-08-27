namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;

/// <summary>
/// The outcome of one <see cref="ILogixTagAccess.WriteAsync"/>. A write produces no data, so this is
/// success or a reason only; what a failure means is the caller's decision.
/// </summary>
internal readonly record struct LogixTagWriteResult
{
    private LogixTagWriteResult(string? error) => Error = error;

    /// <summary>Why the write failed; <c>null</c> when it succeeded.</summary>
    public string? Error { get; }

    /// <summary>Whether the write succeeded.</summary>
    public bool Succeeded => Error is null;

    /// <summary>A write that succeeded.</summary>
    public static LogixTagWriteResult Ok() => new(null);

    /// <summary>A write that failed, carrying why.</summary>
    public static LogixTagWriteResult Failed(string error) => new(error);
}
