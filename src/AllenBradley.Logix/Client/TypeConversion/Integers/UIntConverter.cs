using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// UINT (0xC7): 16-bit little-endian unsigned integer — the same two bytes as an INT, told apart only by
/// the declared type, since nothing on the wire records what the top bit means.
/// </summary>
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
