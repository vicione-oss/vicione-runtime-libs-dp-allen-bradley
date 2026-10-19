using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Timers;

/// <summary>
/// The accumulated value of a timer (<c>T</c>) element — the <c>.ACC</c> word, carried as <see cref="short"/>.
/// The value is in ticks of the time base, because the time base is set in the instruction and not in the
/// data file.
/// </summary>
/// <param name="FileNumber">The number of the timer file.</param>
/// <param name="ElementNumber">The element inside it.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record TimerDataPoint(
    FileNumber FileNumber,
    ElementNumber ElementNumber,
    PollFrequency PollFrequency,
    Channels Channels)
    : LegacyDataPoint<short>(
        new DataFileAddress(DataFileType.Timer, FileNumber, ElementNumber, SubElement: SubElement.Accumulated),
        PollFrequency,
        Channels)
{
    /// <inheritdoc />
    internal override ILegacyDataPointValue<short> CreateLegacyValue(short value) => new Value(this, value);

    // A negative accumulated value is a major fault on Logix (#21); until SLC 500 and MicroLogix are
    // known to tolerate it, it is refused here too.
    private sealed record Value(ILegacyDataPoint DataPoint, short TypedValue) : ILegacyDataPointValue<short>
    {
        public bool IsInValueRange() => TypedValue >= 0;
    }
}
