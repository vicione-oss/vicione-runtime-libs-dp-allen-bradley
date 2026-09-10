using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// ULINT (0xC9): 64-bit little-endian unsigned integer — the same eight bytes as a LINT, told apart only by
/// the declared type, since nothing on the wire records what the top bit means.
/// </summary>
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
