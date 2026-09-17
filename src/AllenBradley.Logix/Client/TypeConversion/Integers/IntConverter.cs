using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// INT (0xC3): 16-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
/// direct read/write with no byte swap.
/// </summary>
internal sealed class IntConverter : DataPointConverter<IntDataPoint, short>
{
    internal const int ElementSize = sizeof(short);

    internal static short DecodeElement(ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadInt16LittleEndian(buffer);

    internal static byte[] EncodeElement(short value)
    {
        var bytes = new byte[ElementSize];
        BinaryPrimitives.WriteInt16LittleEndian(bytes, value);
        return bytes;
    }

    protected override short DecodeValue(IntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DecodeElement(buffer);

    protected override byte[] EncodeValue(IntDataPoint dataPoint, short value) =>
        EncodeElement(value);
}
