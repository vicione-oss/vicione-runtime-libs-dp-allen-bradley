using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Counters;

/// <summary>
/// A COUNTER's accumulated value: the <c>.ACC</c> DINT alone, because the handle reaches that member and
/// not the whole structure.
/// </summary>
internal sealed class CounterConverter : DataPointConverter<CounterDataPoint, int>
{
    protected override int DecodeValue(CounterDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        Int32Codec.Instance.Decode(buffer);

    protected override byte[] EncodeValue(CounterDataPoint dataPoint, int value) =>
        Int32Codec.Instance.Encode(value);
}
