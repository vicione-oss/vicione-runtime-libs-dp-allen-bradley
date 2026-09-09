using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

/// <summary>
/// A one-dimensional Logix <c>INT</c> array tag, read whole and carried as one <see cref="short"/> array
/// of the declared length.
/// </summary>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
/// <param name="ElementCount">The number of elements the tag is declared with in Studio 5000.</param>
public sealed record IntArrayDataPoint(
    TagName TagName, PollFrequency PollFrequency, Channels Channels, ElementCount ElementCount)
    : LogixDataPoint<short[]>(TagName, PollFrequency, Channels), ILogixArrayDataPoint
{
    /// <inheritdoc />
    protected override LogixDataTypeName TypeName => LogixDataTypeName.IntArray;

    /// <inheritdoc />
    internal override ILogixDataPointValue<short[]> CreateLogixValue(short[] value) => new Value(this, value);

    // The value keeps the concrete point rather than the interface, because the length it is judged
    // against is the point's own configuration.
    private sealed record Value(IntArrayDataPoint IntArrayDataPoint, short[] TypedValue)
        : ILogixDataPointValue<short[]>
    {
        public ILogixDataPoint DataPoint => IntArrayDataPoint;

        // A whole array is one value, so a different length is not a smaller value of this tag — it is a
        // value of some other tag. Every element an INT can hold, the tag can hold.
        public bool IsInValueRange() =>
            TypedValue is not null && TypedValue.Length == IntArrayDataPoint.ElementCount.Value;
    }
}
