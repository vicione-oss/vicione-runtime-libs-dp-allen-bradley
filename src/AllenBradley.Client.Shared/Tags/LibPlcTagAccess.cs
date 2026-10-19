using libplctag;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;

/// <summary>
/// The adapter that binds the client stack to libplctag's native <c>RootTagName</c>: one whole operation per
/// member, so the handle's buffer is never exposed between calls, and <c>LibPlcTagException</c> mapped
/// onto a failed result (ADR/2026-07-16-operations-not-accessors-over-libplctag.md).
/// </summary>
internal sealed class LibPlcTagAccess(Tag tag) : ITagAccess
{
    public async Task<TagReadResult> ReadAsync(CancellationToken cancellationToken)
    {
        try
        {
            await InitializeOnFirstUseAsync(cancellationToken).ConfigureAwait(false);
            await tag.ReadAsync(cancellationToken).ConfigureAwait(false);
            var buffer = tag.GetBuffer();
            return TagReadResult.Ok(buffer);
        }
        catch (LibPlcTagException ex)
        {
            // Deliberately not catching cancellation: that is the caller's decision, not the device's.
            return TagReadResult.Failed($"Read failed for tag '{tag.Name}': {ex.Message}");
        }
    }

    // SetBuffer resizes the handle to the length of the buffer, so the controller receives exactly these bytes.
    // Nothing here checks them against the width of the tag. The converters encode the exact width.
    public async Task<TagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        try
        {
            await InitializeOnFirstUseAsync(cancellationToken).ConfigureAwait(false);
            tag.SetBuffer(buffer);
            await tag.WriteAsync(cancellationToken).ConfigureAwait(false);
            return TagWriteResult.Ok();
        }
        catch (LibPlcTagException ex)
        {
            return TagWriteResult.Failed($"Write failed for tag '{tag.Name}': {ex.Message}");
        }
    }

    // The wrapper initializes a handle in ReadAsync, but its buffer accessors fail on a handle that is not
    // initialized (ErrorNotFound). A write sets the buffer first, so read and write both initialize it here.
    private async Task InitializeOnFirstUseAsync(CancellationToken cancellationToken)
    {
        if (!tag.IsInitialized)
        {
            await tag.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public void Dispose() => tag.Dispose();
}
