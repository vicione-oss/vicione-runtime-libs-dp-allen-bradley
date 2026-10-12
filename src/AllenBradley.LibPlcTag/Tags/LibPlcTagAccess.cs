using libplctag;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.Tags;

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

    // SetBuffer copies into the handle's own buffer, which is the controller's width for the tag. The
    // core bounds-checks the copy (plc_tag_set_raw_bytes, libplctag 2.6.3), so a buffer longer than the
    // handle is refused before anything is sent and lands in the catch below; a shorter one fills the
    // handle from the start and leaves the rest as it was.
    public async Task<TagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        try
        {
            tag.SetBuffer(buffer);
            await tag.WriteAsync(cancellationToken).ConfigureAwait(false);
            return TagWriteResult.Ok();
        }
        catch (LibPlcTagException ex)
        {
            return TagWriteResult.Failed($"Write failed for tag '{tag.Name}': {ex.Message}");
        }
    }

    public void Dispose() => tag.Dispose();
}
