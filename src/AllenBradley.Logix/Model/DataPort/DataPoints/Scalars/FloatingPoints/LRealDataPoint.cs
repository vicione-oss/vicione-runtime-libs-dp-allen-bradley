using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

/// <summary>A Logix <c>LREAL</c> tag — an IEEE-754 double, carried as <see cref="double"/>.</summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record LRealDataPoint(TagName TagName, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<double>(TagName, PollFrequency, Channels)
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
