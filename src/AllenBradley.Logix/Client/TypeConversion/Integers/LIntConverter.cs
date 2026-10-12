using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// LINT (0xC5): 64-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
/// direct read/write with no byte swap.
/// </summary>
internal sealed class LIntConverter : DataPointConverter<LIntDataPoint, long>
{
    private readonly Int64Codec _codec = new();

    protected override long DecodeValue(LIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(LIntDataPoint dataPoint, long value) =>
        _codec.Encode(value);
}
