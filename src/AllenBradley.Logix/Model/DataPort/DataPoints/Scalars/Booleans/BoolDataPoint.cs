using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;

/// <summary>A Logix <c>BOOL</c> tag — the atomic one byte, carried as <see cref="bool"/>.</summary>
/// <param name="TagPath">Where the tag's value lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record BoolDataPoint(TagPath TagPath, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<bool>(TagPath, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Bool;

    /// <inheritdoc />
    internal override ILogixDataPointValue<bool> CreateLogixValue(bool value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, bool TypedValue) : ILogixDataPointValue<bool>
    {
        public bool IsInValueRange() => true;
    }
}
