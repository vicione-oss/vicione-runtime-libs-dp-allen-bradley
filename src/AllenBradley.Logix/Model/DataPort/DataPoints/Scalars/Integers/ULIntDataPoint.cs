using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>ULINT</c> tag — a 64-bit unsigned integer, carried as <see cref="ulong"/>.</summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record ULIntDataPoint(TagName TagName, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<ulong>(TagName, PollFrequency, Channels)
{
    /// <inheritdoc />
    protected override LogixDataTypeName TypeName => LogixDataTypeName.ULInt;

    /// <inheritdoc />
    internal override ILogixDataPointValue<ulong> CreateLogixValue(ulong value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, ulong TypedValue) : ILogixDataPointValue<ulong>
    {
        public bool IsInValueRange() => true;
    }
}
