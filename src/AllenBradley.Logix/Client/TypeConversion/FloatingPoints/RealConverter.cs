using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

// REAL (0xCA): IEEE-754 single-precision, little-endian. Direct read/write, no byte swap.
internal sealed class RealConverter : AtomicDataPointConverter<RealDataPoint, float>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.Real;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Real;

    protected override float DecodeValue(RealDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadSingleLittleEndian(buffer);

    protected override void EncodeValue(RealDataPoint dataPoint, float value, Span<byte> buffer) =>
        BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
}
