using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// INT (0xC3): 16-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
/// direct read/write with no byte swap.
/// </summary>
internal sealed class IntConverter : DataPointConverter<IntDataPoint, short>
{
    private readonly Int16Codec _codec = new();

    protected override short DecodeValue(IntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(IntDataPoint dataPoint, short value) =>
        _codec.Encode(value);
}
