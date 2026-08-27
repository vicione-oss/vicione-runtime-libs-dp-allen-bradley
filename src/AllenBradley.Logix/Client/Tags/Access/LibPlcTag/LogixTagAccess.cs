using libplctag;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.Tags.Access.LibPlcTag;

// ILogixTagAccess over a libplctag Tag — the adapter that binds the client stack to the native
// library (ADR-001). Each member runs one whole exchange (read + size + buffer out, buffer in +
// write) so the native handle's buffer is never exposed between calls, and maps LibPlcTagException
// onto a failed result, so the layers above see neither the library's exception type nor its buffer
// protocol. No batching happens here: the native core packs same-connection requests into a
// Multiple Service Packet on its own.
internal sealed class LogixTagAccess(Tag tag) : ILogixTagAccess
{
    public async Task<LogixTagReadResult> ReadAsync(CancellationToken cancellationToken)
    {
        try
        {
            await tag.ReadAsync(cancellationToken).ConfigureAwait(false);
            var buffer = tag.GetBuffer();
            return LogixTagReadResult.Ok(buffer);
        }
        catch (LibPlcTagException ex)
        {
            // The reason travels as data; cancellation is not caught here — it is the caller's
            // decision, not the device's answer.
            return LogixTagReadResult.Failed($"Read failed for tag '{tag.Name}': {ex.Message}");
        }
    }

    public async Task<LogixTagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        try
        {
            tag.SetBuffer(buffer);
            await tag.WriteAsync(cancellationToken).ConfigureAwait(false);
            return LogixTagWriteResult.Ok();
        }
        catch (LibPlcTagException ex)
        {
            return LogixTagWriteResult.Failed($"Write failed for tag '{tag.Name}': {ex.Message}");
        }
    }

    public void Dispose() => tag.Dispose();
}
