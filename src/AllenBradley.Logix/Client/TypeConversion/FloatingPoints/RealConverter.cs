using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

/// <summary>
/// REAL (0xCA): IEEE-754 single-precision, little-endian — a direct read/write with no byte swap.
/// </summary>
internal sealed class RealConverter : AtomicDataPointConverter<RealDataPoint, float>
{
    internal const int ElementSize = sizeof(float);

    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.Real;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Real;

    internal static float DecodeElement(ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadSingleLittleEndian(buffer);

    internal static byte[] EncodeElement(float value)
    {
        var bytes = new byte[ElementSize];
        BinaryPrimitives.WriteSingleLittleEndian(bytes, value);
        return bytes;
    }

    protected override float DecodeValue(RealDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DecodeElement(buffer);

    protected override byte[] EncodeValue(RealDataPoint dataPoint, float value) =>
        EncodeElement(value);
}
