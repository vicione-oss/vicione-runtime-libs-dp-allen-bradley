using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// UDINT (0xC8): 32-bit little-endian unsigned integer — the same four bytes as a DINT, told apart only by
/// the declared type, since nothing on the wire records what the top bit means.
/// </summary>
internal sealed class UDIntConverter : DataPointConverter<UDIntDataPoint, uint>
{
    internal const int ElementSize = sizeof(uint);

    internal static uint DecodeElement(ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadUInt32LittleEndian(buffer);

    internal static byte[] EncodeElement(uint value)
    {
        var bytes = new byte[ElementSize];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        return bytes;
    }

    protected override uint DecodeValue(UDIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DecodeElement(buffer);

    protected override byte[] EncodeValue(UDIntDataPoint dataPoint, uint value) =>
        EncodeElement(value);
}
