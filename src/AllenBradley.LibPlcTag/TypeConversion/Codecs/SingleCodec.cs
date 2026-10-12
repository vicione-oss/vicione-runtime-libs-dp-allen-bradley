using System.Buffers.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

/// <summary>An IEEE-754 single-precision float, little-endian.</summary>
internal sealed class SingleCodec : ElementCodec<float>
{
    internal static readonly SingleCodec Instance = new();

    private SingleCodec()
    {
    }

    internal override int Size => sizeof(float);

    internal override float Decode(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadSingleLittleEndian(buffer);

    internal override void Encode(float value, Span<byte> buffer) => BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
}
