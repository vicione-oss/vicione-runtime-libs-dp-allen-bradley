namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tags;

/// <summary>
/// Serializes operations on one shared access: libplctag rejects concurrent operations on a single
/// handle with a busy status, and its buffer access and device sync are separate calls that only a gated
/// whole operation joins (ADR/2026-07-16-operations-not-accessors-over-libplctag.md). Separate accesses
/// do not contend, so fan-out across tags is untouched.
/// </summary>
internal sealed class SynchronizedTagAccess(ITagAccess inner) : ITagAccess
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<TagReadResult> ReadAsync(CancellationToken cancellationToken)
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

    public async Task<TagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
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

    // Not gated: waiting on the gate here would trade a fail-fast for a deadlock when an operation
    // never completes.
    public void Dispose()
    {
        inner.Dispose();
        _gate.Dispose();
    }
}
