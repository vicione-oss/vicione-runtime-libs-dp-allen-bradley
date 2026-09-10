using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;

/// <summary>A Logix <c>BOOL</c> tag — the atomic one byte, carried as <see cref="bool"/>.</summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record BoolDataPoint(TagName TagName, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<bool>(TagName, PollFrequency, Channels)
{
    /// <inheritdoc />
    protected override LogixDataTypeName TypeName => LogixDataTypeName.Bool;

    /// <inheritdoc />
    internal override ILogixDataPointValue<bool> CreateLogixValue(bool value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, bool TypedValue) : ILogixDataPointValue<bool>
    {
        public bool IsInValueRange() => true;
    }
}
