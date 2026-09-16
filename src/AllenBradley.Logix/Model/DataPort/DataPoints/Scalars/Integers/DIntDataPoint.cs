using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

/// <summary>A Logix <c>DINT</c> tag — a 32-bit signed integer, carried as <see cref="int"/>.</summary>
/// <param name="TagAddress">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public sealed record DIntDataPoint(TagAddress TagAddress, PollFrequency PollFrequency, Channels Channels)
    : LogixDataPoint<int>(TagAddress, PollFrequency, Channels)
{
    /// <inheritdoc />
    public override AllenBradleyDataType DataType => AllenBradleyDataType.Dint;

    /// <inheritdoc />
    internal override ILogixDataPointValue<int> CreateLogixValue(int value) => new Value(this, value);

    private sealed record Value(ILogixDataPoint DataPoint, int TypedValue) : ILogixDataPointValue<int>
    {
        public bool IsInValueRange() => true;
    }
}
