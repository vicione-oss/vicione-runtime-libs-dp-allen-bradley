namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags;

/// <summary>
/// The outcome of one <see cref="Access.ILogixTagAccess.WriteAsync"/>. A write produces no data, so this
/// is success or a reason only.
/// </summary>
internal readonly record struct LogixTagWriteResult
{
    private LogixTagWriteResult(string? error) => Error = error;

    /// <summary>Why the write failed; <c>null</c> when it succeeded.</summary>
    public string? Error { get; }

    public bool Succeeded => Error is null;

    public static LogixTagWriteResult Ok() => new(null);

    public static LogixTagWriteResult Failed(string error) => new(error);
}
