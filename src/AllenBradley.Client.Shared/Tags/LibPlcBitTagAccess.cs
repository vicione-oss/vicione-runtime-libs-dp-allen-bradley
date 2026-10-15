using libplctag;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.Tags;

/// <summary>
/// The adapter for a tag whose name selects one bit, such as <c>B3:2/5</c>. libplctag refuses the buffer of
/// such a tag, so this access carries the bit as the one byte of <see cref="BitCodec"/> and hands it to the
/// rest of the client like any other buffer (docs/AllenBradley.Documentation/libplctag/writing-a-single-bit.md).
/// </summary>
internal sealed class LibPlcBitTagAccess(Tag tag) : ITagAccess
{
    // libplctag takes the bit number from the tag name and ignores the offset of GetBit and SetBit.
    private const int BitOfTheName = 0;

    private readonly BitCodec _codec = new();

    public async Task<TagReadResult> ReadAsync(CancellationToken cancellationToken)
    {
        try
        {
            await tag.ReadAsync(cancellationToken).ConfigureAwait(false);
            return TagReadResult.Ok(_codec.Encode(tag.GetBit(BitOfTheName)));
        }
        catch (LibPlcTagException ex)
        {
            return TagReadResult.Failed($"Read failed for tag '{tag.Name}': {ex.Message}");
        }
    }

    public async Task<TagWriteResult> WriteAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        if (buffer.Length != _codec.Size)
        {
            return TagWriteResult.Failed(
                $"Write failed for tag '{tag.Name}': a bit is written from {_codec.Size} byte, not {buffer.Length}.");
        }

        try
        {
            tag.SetBit(BitOfTheName, _codec.Decode(buffer));
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
