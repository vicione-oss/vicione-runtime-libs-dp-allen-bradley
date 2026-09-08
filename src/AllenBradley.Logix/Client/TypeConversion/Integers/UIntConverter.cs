using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

// UINT (0xC7): 16-bit little-endian unsigned integer. CIP and .NET are both little-endian, so this is
// a direct read/write with no byte swap. It occupies the same two bytes as an INT and differs only in
// what the top bit means, which nothing on the wire records — ExpectedDataType is what keeps an INT
// tag from being read as this and handing back 65535 where the controller holds -1.
internal sealed class UIntConverter : AtomicDataPointConverter<UIntDataPoint, ushort>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.UInt;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Uint;

    protected override ushort DecodeValue(UIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadUInt16LittleEndian(buffer);

    protected override byte[] EncodeValue(UIntDataPoint dataPoint, ushort value)
    {
        var bytes = new byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
        return bytes;
    }
}
