using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// UDINT (0xC8): 32-bit little-endian unsigned integer — the same four bytes as a DINT, told apart only by
/// the declared type, since nothing on the wire records what the top bit means.
/// </summary>
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
