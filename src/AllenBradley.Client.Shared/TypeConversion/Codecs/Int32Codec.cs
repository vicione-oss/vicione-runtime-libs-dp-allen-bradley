using System.Buffers.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

/// <summary>A 32-bit signed integer, little-endian, in two's complement.</summary>
internal sealed class Int32Codec : ElementCodec<int>
{
    internal override int Size => sizeof(int);

    internal override int Decode(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt32LittleEndian(buffer);

    internal override void Encode(int value, Span<byte> buffer) => BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
}
