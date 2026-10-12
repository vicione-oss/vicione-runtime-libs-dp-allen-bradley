using System.Buffers.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

/// <summary>A 64-bit signed integer, little-endian, in two's complement.</summary>
internal sealed class Int64Codec : ElementCodec<long>
{
    internal override int Size => sizeof(long);

    internal override long Decode(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt64LittleEndian(buffer);

    internal override void Encode(long value, Span<byte> buffer) => BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
}
