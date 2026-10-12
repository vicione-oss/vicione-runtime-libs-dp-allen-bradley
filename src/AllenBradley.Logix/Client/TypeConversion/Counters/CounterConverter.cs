using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Counters;

/// <summary>
/// A COUNTER's accumulated value: the <c>.ACC</c> DINT alone, because the handle reaches that member and
/// not the whole structure.
/// </summary>
internal sealed class CounterConverter : DataPointConverter<CounterDataPoint, int>
{
    private readonly Int32Codec _codec = new();

    protected override int DecodeValue(CounterDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(CounterDataPoint dataPoint, int value) =>
        _codec.Encode(value);
}
