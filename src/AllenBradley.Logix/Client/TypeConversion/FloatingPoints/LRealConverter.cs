using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

// LREAL (0xCB): IEEE-754 double-precision, little-endian. Direct read/write, no byte swap.
internal sealed class LRealConverter : AtomicDataPointConverter<LRealDataPoint, double>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.LReal;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Lreal;

    protected override double DecodeValue(LRealDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadDoubleLittleEndian(buffer);

    protected override void EncodeValue(LRealDataPoint dataPoint, double value, Span<byte> buffer) =>
        BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
}
