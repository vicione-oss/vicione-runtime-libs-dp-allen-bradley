using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// DINT (0xC4): 32-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
/// direct read/write with no byte swap.
/// </summary>
internal sealed class DIntConverter : DataPointConverter<DIntDataPoint, int>
{
    internal const int ElementSize = sizeof(int);

    internal static int DecodeElement(ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadInt32LittleEndian(buffer);

    internal static byte[] EncodeElement(int value)
    {
        var bytes = new byte[ElementSize];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        return bytes;
    }

    protected override int DecodeValue(DIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DecodeElement(buffer);

    protected override byte[] EncodeValue(DIntDataPoint dataPoint, int value) =>
        EncodeElement(value);
}
