using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.FloatingPoints;

/// <summary>A Logix <c>REAL</c> tag — an IEEE-754 single, carried as <see cref="float"/>.</summary>
/// <param name="TagPath">Where the tag's value lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record RealDataPoint(TagPath TagPath, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<float>(TagPath, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Real;

    /// <inheritdoc />
    internal override ILogixDataPointValue<float> CreateLogixValue(float value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, float TypedValue) : ILogixDataPointValue<float>
    {
        public bool IsInValueRange() => true;
    }
}
