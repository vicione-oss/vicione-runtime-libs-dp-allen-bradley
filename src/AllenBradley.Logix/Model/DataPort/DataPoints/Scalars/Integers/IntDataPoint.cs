using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>INT</c> tag — a 16-bit signed integer, carried as <see cref="short"/>.</summary>
/// <param name="TagAddress">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record IntDataPoint(TagAddress TagAddress, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<short>(TagAddress, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Int;

    /// <inheritdoc />
    internal override ILogixDataPointValue<short> CreateLogixValue(short value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, short TypedValue) : ILogixDataPointValue<short>
    {
        public bool IsInValueRange() => true;
    }
}
