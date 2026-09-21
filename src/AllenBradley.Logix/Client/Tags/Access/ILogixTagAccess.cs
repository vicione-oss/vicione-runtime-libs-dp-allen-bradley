namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;

/// <summary>
/// Read/write access to one tag on the controller — the mockable seam over libplctag's sealed, native
/// <c>RootTagName</c>, with one whole operation per member and no overlap between them
/// (ADR/2026-07-16-operations-not-accessors-over-libplctag.md). A device failure is a failed result, not
/// an exception; only cancellation throws.
/// </summary>
internal interface ILogixTagAccess : IDisposable
{
    /// <summary>Reads the tag from the controller and returns its raw bytes.</summary>
    Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Writes <paramref name="buffer"/> to the tag on the controller. The buffer is the value's own
    /// width; one longer than the handle is a failed result, one shorter fills the handle from the start
    /// and leaves its tail as it was.
    /// </summary>
    Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken);
}
