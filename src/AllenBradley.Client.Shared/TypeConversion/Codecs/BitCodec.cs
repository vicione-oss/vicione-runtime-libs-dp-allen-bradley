namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

/// <summary>
/// One bit of a bit tag, in the one byte that <see cref="Tags.LibPlcBitTagAccess"/> carries it as. This is
/// not a wire format: libplctag refuses the buffer of a bit tag, so the access reads and writes the bit
/// and this codec turns it into a byte and back.
/// </summary>
internal sealed class BitCodec : ElementCodec<bool>
{
    private const byte Set = 0x01;
    private const byte Clear = 0x00;

    internal override int Size => sizeof(byte);

    internal override bool Decode(ReadOnlySpan<byte> buffer) => buffer[0] != Clear;

    internal override void Encode(bool value, Span<byte> buffer) => buffer[0] = value ? Set : Clear;
}
