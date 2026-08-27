using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// REAL (0xCA): IEEE-754 single-precision, little-endian. Direct read/write, no byte swap.
internal sealed class RealConverter : DataPointConverter<RealDataPoint, float>
{
    public override AllenBradleyDataType ExpectedType => AllenBradleyDataType.Real;

    public override ByteSize ByteSize => new(sizeof(float));

    protected override float DecodeValue(RealDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadSingleLittleEndian(buffer);

    protected override void EncodeValue(RealDataPoint dataPoint, float value, Span<byte> buffer) =>
        BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
}
