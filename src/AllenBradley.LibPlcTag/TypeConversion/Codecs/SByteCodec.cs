namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

/// <summary>
/// An 8-bit signed integer: one byte, so there is no byte order to get wrong, and the bit pattern is the
/// two's-complement value as it stands.
/// </summary>
internal sealed class SByteCodec : ElementCodec<sbyte>
{
    internal static readonly SByteCodec Instance = new();

    private SByteCodec()
    {
    }

    internal override int Size => sizeof(sbyte);

    internal override sbyte Decode(ReadOnlySpan<byte> buffer) => (sbyte)buffer[0];

    internal override void Encode(sbyte value, Span<byte> buffer) => buffer[0] = (byte)value;
}
