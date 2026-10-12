using System.Buffers.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

/// <summary>A 32-bit unsigned integer, little-endian.</summary>
internal sealed class UInt32Codec : ElementCodec<uint>
{
    internal override int Size => sizeof(uint);

    internal override uint Decode(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt32LittleEndian(buffer);

    internal override void Encode(uint value, Span<byte> buffer) => BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
}
