namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

/// <summary>An 8-bit unsigned integer: one byte, as it stands.</summary>
internal sealed class ByteCodec : ElementCodec<byte>
{
    internal static readonly ByteCodec Instance = new();

    private ByteCodec()
    {
    }

    internal override int Size => sizeof(byte);

    internal override byte Decode(ReadOnlySpan<byte> buffer) => buffer[0];

    internal override void Encode(byte value, Span<byte> buffer) => buffer[0] = value;
}
