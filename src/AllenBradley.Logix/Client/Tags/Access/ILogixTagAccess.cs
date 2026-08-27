namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;

/// <summary>
/// Read/write access to one tag on the controller — the mockable seam over libplctag's sealed, native
/// <c>Tag</c>. Each member is one whole exchange: a read carries its bytes home, a write takes its bytes in.
/// <see cref="LibPlcTag.LogixTagAccess"/> is the production adapter; tests substitute in-process fakes.
/// </summary>
/// <remarks>
/// One access may be shared across callers, and operations must not overlap on it —
/// <see cref="SynchronizedLogixTagAccess"/> enforces that, and ADR-001 records why whole-exchange
/// members are what make the gating sound.
/// <para>
/// A device failure is a failed result, not an exception — a tag that will not read is an ordinary
/// event in a polling group. Cancellation still throws.
/// </para>
/// </remarks>
internal interface ILogixTagAccess : IDisposable
{
    /// <summary>Reads the tag from the controller and returns its raw bytes.</summary>
    Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken);

    /// <summary>Writes <paramref name="buffer"/> to the tag on the controller.</summary>
    Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken);

    public byte[] CreateNewWriteBuffer();
}
