using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Timers;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>
/// Converts the accumulated value of a timer (<c>T</c>) element, one 16-bit word, to a <see cref="short"/> and back.
/// </summary>
internal sealed class TimerConverter : LegacyDataPointConverter<TimerDataPoint, short>
{
    private readonly Int16Codec _codec = new();

    protected override short DecodeValue(TimerDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(TimerDataPoint dataPoint, short value) => _codec.Encode(value);
}
