using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

/// <summary>
/// REAL (0xCA): IEEE-754 single-precision, little-endian — a direct read/write with no byte swap.
/// </summary>
internal sealed class RealConverter : DataPointConverter<RealDataPoint, float>
{
    private readonly SingleCodec _codec = new();

    protected override float DecodeValue(RealDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(RealDataPoint dataPoint, float value) =>
        _codec.Encode(value);
}
