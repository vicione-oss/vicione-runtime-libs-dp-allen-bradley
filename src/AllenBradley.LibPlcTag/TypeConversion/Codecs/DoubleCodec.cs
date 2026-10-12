using System.Buffers.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;

/// <summary>An IEEE-754 double-precision float, little-endian.</summary>
internal sealed class DoubleCodec : ElementCodec<double>
{
    internal override int Size => sizeof(double);

    internal override double Decode(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadDoubleLittleEndian(buffer);

    internal override void Encode(double value, Span<byte> buffer) => BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
}
