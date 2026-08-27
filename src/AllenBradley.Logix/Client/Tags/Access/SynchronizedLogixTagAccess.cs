namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access;

// One operation at a time on a shared access. The cache hands the same access to every data point
// naming the tag, so a read and a write can reach it at once — and overlapping operations on one
// libplctag handle mispair completions inside the .NET wrapper and cascade into timeouts, while its
// raw buffer accessors can race in the gaps between operations. Gating whole ILogixTagAccess
// members closes both races at once; both are verified against the device (ADR-001). A separate
// access never meets this gate, so a group's fan-out across tags is untouched.
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
