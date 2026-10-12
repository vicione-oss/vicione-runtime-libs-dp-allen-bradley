using System.Buffers.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

/// <summary>A 16-bit unsigned integer, little-endian.</summary>
internal sealed class UInt16Codec : ElementCodec<ushort>
{
    internal override int Size => sizeof(ushort);

    internal override ushort Decode(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt16LittleEndian(buffer);

    internal override void Encode(ushort value, Span<byte> buffer) => BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
}
