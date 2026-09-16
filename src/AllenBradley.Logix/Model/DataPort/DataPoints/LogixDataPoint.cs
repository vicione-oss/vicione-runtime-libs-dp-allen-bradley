using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// What every Logix data point carries whatever its type: the tag it addresses, how often it is polled,
/// the channels it feeds, and the .NET type it exchanges.
/// </summary>
/// <typeparam name="TDomain">The .NET type this point exchanges — <c>int</c> for a <c>DINT</c>.</typeparam>
/// <param name="TagAddress">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public abstract record LogixDataPoint<TDomain>(TagAddress TagAddress, PollFrequency PollFrequency, Channels Channels)
    : ILogixDataPoint, ITypedDataPoint<TDomain>
{
    /// <inheritdoc />
    public DataPointIdentifier Identifier => new(TagAddress.Value);

    /// <inheritdoc />
    public DataTypeName DataTypeName => new(TypeName);

    /// <summary>The type this point holds; for an array, the element type.</summary>
    public abstract AllenBradleyDataType DataType { get; }

    /// <summary>This point's type as Studio 5000 spells it, with the shape a scalar does not have.</summary>
    protected virtual string TypeName => DataType.Name;

    /// <inheritdoc />
    public ITypedDataPointValue<TDomain> CreateTypedValue(TDomain value) => CreateLogixValue(value);

    /// <inheritdoc />
    public DataPointValueConversion<ILogixDataPointValue> ConvertValue(object? value) => value switch
    {
        null => ConversionFailure("value was null"),

        // The pattern binds the value; the exact-type test is the rule. The CLR holds arrays of
        // same-width signed and unsigned elements assignment-compatible, so the pattern alone lets a
        // ushort[] through as a short[] and writes 40000 to the controller as -25536. Dropping the
        // pattern for a (TDomain) cast is not the shorter spelling it looks like: the same rule makes
        // that cast succeed, so the test would be all that stands there. Boxed scalars are exact either
        // way.
        TDomain typed when value.GetType() == typeof(TDomain) =>
            new ConvertedDataPointValue<ILogixDataPointValue>(CreateLogixValue(typed)),

        _ => ConversionFailure($"value of type {value.GetType()} is not assignable to {typeof(TDomain)}"),
    };

    /// <summary>
    /// Wraps <paramref name="value"/> in this point's own value record — the narrower twin of
    /// <see cref="CreateTypedValue"/>, which the converters decode straight into so the read path never
    /// casts the framework's view back to ours.
    /// </summary>
    internal abstract ILogixDataPointValue<TDomain> CreateLogixValue(TDomain value);

    // A rejected write is diagnosed from this message and nothing else: the engine value that caused it
    // is gone by the time anyone reads the log line.
    private NotConvertedDataPointValue<ILogixDataPointValue> ConversionFailure(string detail) =>
        new(new DataPointValueValidationFailure(
            this,
            ValidationFailureReason.ConversionFailure,
            $"Could not convert value at {Identifier} to {DataTypeName.Value}: {detail}"));
}
