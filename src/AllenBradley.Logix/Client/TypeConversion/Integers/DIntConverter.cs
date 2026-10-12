using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// DINT (0xC4): 32-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
/// direct read/write with no byte swap.
/// </summary>
internal sealed class DIntConverter : DataPointConverter<DIntDataPoint, int>
{
    protected override int DecodeValue(DIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        Int32Codec.Instance.Decode(buffer);

    protected override byte[] EncodeValue(DIntDataPoint dataPoint, int value) =>
        Int32Codec.Instance.Encode(value);
}
