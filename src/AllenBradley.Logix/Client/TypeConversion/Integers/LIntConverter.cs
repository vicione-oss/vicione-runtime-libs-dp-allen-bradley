using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// LINT (0xC5): 64-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
/// direct read/write with no byte swap.
/// </summary>
internal sealed class LIntConverter : DataPointConverter<LIntDataPoint, long>
{
    internal const int ElementSize = sizeof(long);

    internal static long DecodeElement(ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadInt64LittleEndian(buffer);

    internal static byte[] EncodeElement(long value)
    {
        var bytes = new byte[ElementSize];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        return bytes;
    }

    protected override long DecodeValue(LIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DecodeElement(buffer);

    protected override byte[] EncodeValue(LIntDataPoint dataPoint, long value) =>
        EncodeElement(value);
}
