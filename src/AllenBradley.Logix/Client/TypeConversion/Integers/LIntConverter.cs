using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// LINT (0xC5): 64-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
/// direct read/write with no byte swap.
/// </summary>
internal sealed class LIntConverter : AtomicDataPointConverter<LIntDataPoint, long>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.LInt;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Lint;

    protected override long DecodeValue(LIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadInt64LittleEndian(buffer);

    protected override byte[] EncodeValue(LIntDataPoint dataPoint, long value)
    {
        var bytes = new byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        return bytes;
    }
}
