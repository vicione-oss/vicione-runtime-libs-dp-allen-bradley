using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

// UDINT (0xC8): 32-bit little-endian unsigned integer. CIP and .NET are both little-endian, so this is
// a direct read/write with no byte swap. It occupies the same four bytes as a DINT and differs only in
// what the top bit means, which nothing on the wire records — ExpectedDataType is what keeps a DINT tag
// from being read as this and handing back 4294967295 where the controller holds -1.
internal sealed class UDIntConverter : AtomicDataPointConverter<UDIntDataPoint, uint>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.UDInt;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Udint;

    protected override uint DecodeValue(UDIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadUInt32LittleEndian(buffer);

    protected override byte[] EncodeValue(UDIntDataPoint dataPoint, uint value)
    {
        var bytes = new byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        return bytes;
    }
}
