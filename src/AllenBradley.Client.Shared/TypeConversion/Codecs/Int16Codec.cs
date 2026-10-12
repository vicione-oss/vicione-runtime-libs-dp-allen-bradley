using System.Buffers.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

/// <summary>A 16-bit signed integer, little-endian, in two's complement.</summary>
internal sealed class Int16Codec : ElementCodec<short>
{
    internal override int Size => sizeof(short);

    internal override short Decode(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt16LittleEndian(buffer);

    internal override void Encode(short value, Span<byte> buffer) => BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
}
