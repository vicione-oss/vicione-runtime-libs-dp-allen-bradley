using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

/// <summary>A Logix <c>LREAL</c> tag — an IEEE-754 double, carried as <see cref="double"/>.</summary>
/// <param name="TagPath">Where the tag's value lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record LRealDataPoint(TagPath TagPath, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<double>(TagPath, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Lreal;

    /// <inheritdoc />
    internal override ILogixDataPointValue<double> CreateLogixValue(double value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, double TypedValue) : ILogixDataPointValue<double>
    {
        public bool IsInValueRange() => true;
    }
}
