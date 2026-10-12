using System.Buffers.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;

/// <summary>A 64-bit unsigned integer, little-endian.</summary>
internal sealed class UInt64Codec : ElementCodec<ulong>
{
    internal override int Size => sizeof(ulong);

    internal override ulong Decode(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt64LittleEndian(buffer);

    internal override void Encode(ulong value, Span<byte> buffer) => BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
}
