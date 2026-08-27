namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// Typed <see cref="ILogixDataPointValue"/>. Built by a converter, which knows the domain type
/// <typeparamref name="TDomain"/> the point carries, so the read path needs no second runtime cast.
/// </summary>
/// <typeparam name="TDomain">The .NET type this data point exchanges (e.g. <c>int</c> for a DINT).</typeparam>
/// <param name="dataPoint">The originating data point.</param>
/// <param name="value">The typed value; <c>default</c> for a Bad result.</param>
/// <param name="quality">The value's quality.</param>
public sealed class LogixDataPointValue<TDomain>(
    ILogixDataPoint dataPoint, TDomain? value, LogixQuality quality) : ILogixDataPointValue
{
    /// <inheritdoc />
    public ILogixDataPoint DataPoint { get; } = dataPoint;

    /// <summary>The typed value.</summary>
    public TDomain? Value { get; } = value;

    /// <inheritdoc />
    public LogixQuality Quality { get; } = quality;

    /// <inheritdoc />
    object? ILogixDataPointValue.Value => Value;
}
