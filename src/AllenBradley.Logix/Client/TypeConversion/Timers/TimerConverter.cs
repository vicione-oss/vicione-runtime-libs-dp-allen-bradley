using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Timers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Timers;

/// <summary>
/// A TIMER's accumulated time: the <c>.ACC</c> DINT alone, because the handle reaches that member and not
/// the whole structure.
/// </summary>
internal sealed class TimerConverter : DataPointConverter<TimerDataPoint, int>
{
    private readonly Int32Codec _codec = new();

    protected override int DecodeValue(TimerDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    // Checked again here, not only by the outgoing port's range validation, because the client can be
    // written to directly.
    protected override byte[] EncodeValue(TimerDataPoint dataPoint, int value)
    {
        if (!TimerDataPoint.IsWritableAccumulatedTime(value))
        {
            throw new InvalidOperationException(
                $"Cannot write {value} ms to {dataPoint.TagAddress}; " +
                "a negative accumulated time is a major fault on the controller.");
        }

        return _codec.Encode(value);
    }
}
