using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;
using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;

/// <summary>
/// What every one-dimensional Logix array tag carries whatever its element type: the declared length, and
/// the value record that judges a value against it. A concrete point adds only its element type.
/// </summary>
/// <typeparam name="TElement">The .NET type of one element — <c>int</c> for a <c>DINT[n]</c>.</typeparam>
/// <param name="TagPath">Where the tag's value lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
/// <param name="ElementCount">The number of elements the tag is declared with in Studio 5000.</param>
public abstract record LogixArrayDataPoint<TElement>(
    TagPath TagPath,
    PollFrequency PollFrequency,
    Channels Channels,
    ElementCount ElementCount)
    : LogixDataPoint<TElement[]>(TagPath, PollFrequency, Channels), ILogixArrayDataPoint
{
    public override DataTypeName DataTypeName => new(DataType.Name.Value + "[]");

    /// <inheritdoc />
    public sealed override DimensionCount DimensionCount => DimensionCount.OneDimensional;

    /// <inheritdoc />
    internal sealed override ILogixDataPointValue<TElement[]> CreateLogixValue(TElement[] value) =>
        new Value(this, value);

    private sealed record Value(LogixArrayDataPoint<TElement> ArrayDataPoint, TElement[] TypedValue)
        : ILogixDataPointValue<TElement[]>
    {
        public ILogixDataPoint DataPoint => ArrayDataPoint;

        // A whole array is one value: a different length is not an out-of-range value of this tag but a
        // value of some other tag.
        public bool IsInValueRange() => TypedValue.Length == ArrayDataPoint.ElementCount.Value;
    }
}
