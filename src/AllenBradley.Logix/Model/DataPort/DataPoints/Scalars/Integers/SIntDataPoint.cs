using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>SINT</c> tag — an 8-bit signed integer, carried as <see cref="sbyte"/>.</summary>
/// <param name="TagPath">Where the tag's value lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record SIntDataPoint(TagPath TagPath, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<sbyte>(TagPath, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Sint;

    /// <inheritdoc />
    internal override ILogixDataPointValue<sbyte> CreateLogixValue(sbyte value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, sbyte TypedValue) : ILogixDataPointValue<sbyte>
    {
        public bool IsInValueRange() => true;
    }
}
