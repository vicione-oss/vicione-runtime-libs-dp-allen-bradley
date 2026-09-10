using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

/// <summary>
/// LREAL (0xCB): IEEE-754 double-precision, little-endian — a direct read/write with no byte swap.
/// </summary>
internal sealed class LRealConverter : AtomicDataPointConverter<LRealDataPoint, double>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.LReal;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Lreal;

    protected override double DecodeValue(LRealDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadDoubleLittleEndian(buffer);

    protected override byte[] EncodeValue(LRealDataPoint dataPoint, double value)
    {
        var bytes = new byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleLittleEndian(bytes, value);
        return bytes;
    }
}
