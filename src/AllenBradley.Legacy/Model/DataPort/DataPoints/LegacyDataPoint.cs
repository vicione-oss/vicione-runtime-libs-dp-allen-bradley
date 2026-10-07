using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// What every Legacy data point carries whatever its type: the element it addresses, how often it is
/// polled, the channels it feeds, and the .NET type it exchanges. The type is the data file's, so a
/// point never declares one of its own.
/// </summary>
/// <typeparam name="TDomain">The .NET type this point exchanges — <c>short</c> for an integer file.</typeparam>
/// <param name="Address">Where the value lives.</param>
/// <param name="PollFrequency">How often an incoming port reads this point.</param>
/// <param name="Channels">The channels this point's value is routed to.</param>
public abstract record LegacyDataPoint<TDomain>(DataFileAddress Address, PollFrequency PollFrequency, Channels Channels)
    : ILegacyDataPoint, ITypedDataPoint<TDomain>
{
    /// <inheritdoc />
    public DataPointIdentifier Identifier => new(Address.ToString());

    /// <inheritdoc />
    public DataTypeName DataTypeName => Address.FileType.Name;

    /// <inheritdoc />
    public ITypedDataPointValue<TDomain> CreateTypedValue(TDomain value) => CreateLegacyValue(value);

    /// <inheritdoc />
    public DataPointValueConversion<ILegacyDataPointValue> ConvertValue(object? value) => value switch
    {
        null => ConversionFailure("value was null"),
        TDomain typed when value.GetType() == typeof(TDomain) =>
            new ConvertedDataPointValue<ILegacyDataPointValue>(CreateLegacyValue(typed)),
        _ => ConversionFailure($"value of type {value.GetType()} is not assignable to {typeof(TDomain)}"),
    };

    /// <summary>
    /// Wraps <paramref name="value"/> in this point's own value record — the narrower twin of
    /// <see cref="CreateTypedValue"/>.
    /// </summary>
    internal abstract ILegacyDataPointValue<TDomain> CreateLegacyValue(TDomain value);

    // A rejected write is diagnosed from this message and nothing else: the engine value that caused it
    // is gone by the time anyone reads the log line.
    private NotConvertedDataPointValue<ILegacyDataPointValue> ConversionFailure(string detail) =>
        new(new DataPointValueValidationFailure(
            this,
            ValidationFailureReason.ConversionFailure,
            $"Could not convert value at {Identifier} to {DataTypeName.Value}: {detail}"));
}
