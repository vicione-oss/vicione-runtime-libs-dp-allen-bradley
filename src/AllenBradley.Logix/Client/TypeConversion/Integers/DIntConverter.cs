using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

// DINT (0xC4): 32-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
// direct read/write with no byte swap.
internal sealed class DIntConverter : AtomicDataPointConverter<DIntDataPoint, int>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.DInt;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Dint;

    protected override int DecodeValue(DIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadInt32LittleEndian(buffer);

    protected override byte[] EncodeValue(DIntDataPoint dataPoint, int value)
    {
        var bytes = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        return bytes;
    }
}
