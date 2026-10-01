using ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Counters;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Counters;

/// <summary>
/// A COUNTER's accumulated value: the <c>.ACC</c> DINT alone, because the handle reaches that member and
/// not the whole structure.
/// </summary>
internal sealed class CounterConverter : DataPointConverter<CounterDataPoint, int>
{
    protected override int DecodeValue(CounterDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DIntConverter.DecodeElement(buffer);

    protected override byte[] EncodeValue(CounterDataPoint dataPoint, int value) =>
        DIntConverter.EncodeElement(value);
}
