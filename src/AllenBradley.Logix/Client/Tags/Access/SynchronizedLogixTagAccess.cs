namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;

// Serializes concurrent operations on one shared access, because concurrent operations on a single libplctag are not supported (the lib will report a busy-error-status).
// Additionally, there is only one byte-buffer and getting or setting its value and synchronizing with the device are separate calls in the lib and need to be joined to guarantee the desired semantics.
// In ACID terms: isolation from the gate, atomicity from joining buffer access and device sync into
// one gated unit.
// (ADR/2026-07-16-operations-not-accessors-over-libplctag.md). Separate accesses do not contend, so fan-out
// across tags is untouched.
internal sealed class SynchronizedLogixTagAccess(ILogixTagAccess inner) : ILogixTagAccess
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await inner.ReadAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    // Not gated: disposal is the owner's call, and waiting on the gate here would trade a fail-fast
    // for a deadlock if an operation never completes.
    public void Dispose()
    {
        inner.Dispose();
        _gate.Dispose();
    }
}
