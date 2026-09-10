using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>USINT</c> tag — an 8-bit unsigned integer, carried as <see cref="byte"/>.</summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record USIntDataPoint(TagName TagName, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<byte>(TagName, PollFrequency, Channels)
{
    /// <inheritdoc />
    protected override LogixDataTypeName TypeName => LogixDataTypeName.USInt;

    /// <inheritdoc />
    internal override ILogixDataPointValue<byte> CreateLogixValue(byte value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, byte TypedValue) : ILogixDataPointValue<byte>
    {
        public bool IsInValueRange() => true;
    }
}
