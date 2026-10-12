using System.Buffers.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

/// <summary>A 32-bit unsigned integer, little-endian.</summary>
internal sealed class UInt32Codec : ElementCodec<uint>
{
    internal static readonly UInt32Codec Instance = new();

    private UInt32Codec()
    {
    }

    internal override int Size => sizeof(uint);

    internal override uint Decode(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt32LittleEndian(buffer);

    internal override void Encode(uint value, Span<byte> buffer) => BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
}
