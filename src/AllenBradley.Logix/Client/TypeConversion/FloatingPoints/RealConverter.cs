using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

/// <summary>
/// REAL (0xCA): IEEE-754 single-precision, little-endian — a direct read/write with no byte swap.
/// </summary>
internal sealed class RealConverter : DataPointConverter<RealDataPoint, float>
{
    protected override float DecodeValue(RealDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        SingleCodec.Instance.Decode(buffer);

    protected override byte[] EncodeValue(RealDataPoint dataPoint, float value) =>
        SingleCodec.Instance.Encode(value);
}
