namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;

/// <summary>
/// Read/write access to one tag on the controller — the mockable seam over libplctag's sealed, native
/// <c>Tag</c>. Each member is one whole exchange: a read carries its bytes home, a write takes its bytes in.
/// <see cref="LibPlcTag.LogixTagAccess"/> is the production adapter; tests substitute in-process fakes.
/// Operations on one access must not overlap. A device failure is a failed result, not an exception; only
/// cancellation throws.
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
