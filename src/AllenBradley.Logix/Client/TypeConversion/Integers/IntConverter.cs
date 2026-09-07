using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

// INT (0xC3): 16-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
// direct read/write with no byte swap.
internal sealed class IntConverter : AtomicDataPointConverter<IntDataPoint, short>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.Int;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Int;

    protected override short DecodeValue(IntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadInt16LittleEndian(buffer);

    protected override byte[] EncodeValue(IntDataPoint dataPoint, short value)
    {
        var bytes = new byte[sizeof(short)];
        BinaryPrimitives.WriteInt16LittleEndian(bytes, value);
        return bytes;
    }
}
