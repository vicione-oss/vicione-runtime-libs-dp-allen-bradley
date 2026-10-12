using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

/// <summary>
/// LREAL (0xCB): IEEE-754 double-precision, little-endian — a direct read/write with no byte swap.
/// </summary>
internal sealed class LRealConverter : DataPointConverter<LRealDataPoint, double>
{
    protected override double DecodeValue(LRealDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DoubleCodec.Instance.Decode(buffer);

    protected override byte[] EncodeValue(LRealDataPoint dataPoint, double value) =>
        DoubleCodec.Instance.Encode(value);
}
