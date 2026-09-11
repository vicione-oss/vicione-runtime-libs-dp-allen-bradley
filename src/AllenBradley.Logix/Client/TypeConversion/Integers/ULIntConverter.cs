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
    internal const int ElementSize = sizeof(ulong);

    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.ULInt;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Ulint;

    internal static ulong DecodeElement(ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadUInt64LittleEndian(buffer);

    internal static byte[] EncodeElement(ulong value)
    {
        var bytes = new byte[ElementSize];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        return bytes;
    }

    protected override ulong DecodeValue(ULIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DecodeElement(buffer);

    protected override byte[] EncodeValue(ULIntDataPoint dataPoint, ulong value) =>
        EncodeElement(value);
}
