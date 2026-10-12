using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// SINT (0xC2): 8-bit signed integer. One byte, so there is no byte order to get wrong — the bit pattern is
/// the two's-complement sbyte as it stands.
/// </summary>
internal sealed class SIntConverter : DataPointConverter<SIntDataPoint, sbyte>
{
    protected override sbyte DecodeValue(SIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        SByteCodec.Instance.Decode(buffer);

    protected override byte[] EncodeValue(SIntDataPoint dataPoint, sbyte value) =>
        SByteCodec.Instance.Encode(value);
}
