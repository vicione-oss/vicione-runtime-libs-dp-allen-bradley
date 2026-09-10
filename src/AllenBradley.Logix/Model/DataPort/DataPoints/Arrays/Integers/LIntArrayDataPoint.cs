using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

/// <summary>
/// A one-dimensional Logix <c>LINT</c> array tag, read whole and carried as one <see cref="long"/> array
/// of the declared length.
/// </summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
/// <param name="ElementCount">The number of elements the tag is declared with in Studio 5000.</param>
public sealed record LIntArrayDataPoint(
    TagName TagName, PollFrequency PollFrequency, Channels Channels, ElementCount ElementCount)
    : LogixDataPoint<long[]>(TagName, PollFrequency, Channels), ILogixArrayDataPoint
{
    /// <inheritdoc />
    protected override LogixDataTypeName TypeName => LogixDataTypeName.LIntArray;

    /// <inheritdoc />
    internal override ILogixDataPointValue<long[]> CreateLogixValue(long[] value) => new Value(this, value);

    // Keeps the concrete point rather than the interface: the length a value is judged against is the
    // point's own configuration.
    private sealed record Value(LIntArrayDataPoint LIntArrayDataPoint, long[] TypedValue)
        : ILogixDataPointValue<long[]>
    {
        public ILogixDataPoint DataPoint => LIntArrayDataPoint;

        // A whole array is one value: a different length is not an out-of-range value of this tag but a
        // value of some other tag.
        public bool IsInValueRange() =>
            TypedValue is not null && TypedValue.Length == LIntArrayDataPoint.ElementCount.Value;
    }
}
