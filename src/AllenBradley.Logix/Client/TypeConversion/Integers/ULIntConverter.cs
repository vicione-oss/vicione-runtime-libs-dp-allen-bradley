using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

// ULINT (0xC9): 64-bit little-endian unsigned integer. CIP and .NET are both little-endian, so this is
// a direct read/write with no byte swap. It occupies the same eight bytes as a LINT and differs only in
// what the top bit means, which nothing on the wire records — ExpectedDataType is what keeps a LINT tag
// from being read as this and handing back 18446744073709551615 where the controller holds -1.
internal sealed class ULIntConverter : AtomicDataPointConverter<ULIntDataPoint, ulong>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.ULInt;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Ulint;

    protected override ulong DecodeValue(ULIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadUInt64LittleEndian(buffer);

    protected override byte[] EncodeValue(ULIntDataPoint dataPoint, ulong value)
    {
        var bytes = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        return bytes;
    }
}
