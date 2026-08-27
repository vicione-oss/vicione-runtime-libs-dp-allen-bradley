using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// What every Logix data point carries whatever its type: the tag it addresses, how often it is polled,
/// the channels it feeds, and the .NET type it exchanges. Concrete shapes add only their
/// <see cref="TypeName"/>, their value record, and whatever configuration their type needs — a
/// <c>STRING</c>'s declared capacity, and nothing at all for the elementary types.
/// </summary>
/// <remarks>
/// <see cref="Identifier"/> and <see cref="DataTypeName"/> are the framework's two diagnostic strings, and
/// this is the only place they are derived: the identifier is the tag address, the type name is the
/// Studio 5000 spelling. Neither is identity — a converter is still resolved from the data point's
/// concrete type, never from either string.
/// <para>
/// The .NET type is <typeparamref name="TDomain"/> itself rather than a discriminator beside it, which
/// is what makes a value and the point that produced it agree by construction: a point makes only its
/// own <see cref="ILogixDataPointValue{TDomain}"/>, and <see cref="ConvertValue"/> is the one door an
/// untyped engine value comes through.
/// </para>
/// </remarks>
/// <typeparam name="TDomain">The .NET type this point exchanges — <c>int</c> for a <c>DINT</c>.</typeparam>
/// <param name="TagName">The symbolic tag address.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public abstract record LogixDataPoint<TDomain>(TagName TagName, PollFrequency PollFrequency, Channels Channels)
    : ILogixDataPoint, ITypedDataPoint<TDomain>
{
    /// <inheritdoc />
    public DataPointIdentifier Identifier => new(TagName.Value);

    /// <inheritdoc />
    public DataTypeName DataTypeName => new(TypeName.Value);

    /// <summary>
    /// This point's type as Studio 5000 names it. Shared with the converter that decodes it, so a type
    /// has one spelling rather than one per reader.
    /// </summary>
    protected abstract LogixDataTypeName TypeName { get; }

    /// <inheritdoc />
    public ITypedDataPointValue<TDomain> CreateTypedValue(TDomain value) => CreateLogixValue(value);

    /// <inheritdoc />
    public DataPointValueConversion<ILogixDataPointValue> ConvertValue(object? value) => value switch
    {
        null => ConversionFailure("value was null"),
        TDomain typed => new ConvertedDataPointValue<ILogixDataPointValue>(CreateLogixValue(typed)),
        _ => ConversionFailure($"value of type {value.GetType()} is not assignable to {typeof(TDomain)}"),
    };

    /// <summary>
    /// Wraps <paramref name="value"/> in this point's own value record. The narrower twin of
    /// <see cref="CreateTypedValue"/> — the converters decode straight into it, so the read path never
    /// casts the framework's view back to ours.
    /// </summary>
    /// <param name="value">The payload the value carries.</param>
    internal abstract ILogixDataPointValue<TDomain> CreateLogixValue(TDomain value);

    // Names the point and the type it wanted, because a rejected write is diagnosed from the log line
    // and nothing else: the engine value that caused it is gone by the time anyone reads it.
    private NotConvertedDataPointValue<ILogixDataPointValue> ConversionFailure(string detail) =>
        new(new DataPointValueValidationFailure(
            this,
            ValidationFailureReason.ConversionFailure,
            $"Could not convert value at {Identifier} to {DataTypeName.Value}: {detail}"));
}
