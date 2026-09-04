using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

/// <summary>
/// The result of reading (or the payload for writing) one data point: the originating point, the
/// value, and its quality. Two shapes implement it — a typed
/// <see cref="ILogixDataPointValue{TDomain}"/> that a data point made from a payload, and
/// <see cref="BadLogixDataPointValue"/>, which carries none.
/// </summary>
public interface ILogixDataPointValue : IDataPointValue
{
    /// <summary>The data point this value belongs to.</summary>
    new ILogixDataPoint DataPoint { get; }

    /// <summary>
    /// Whether the value is trustworthy. <see cref="IDataPointValue.Value"/> is not meaningful when this
    /// is <see cref="LogixQuality.Bad"/>.
    /// </summary>
    LogixQuality Quality { get; }

    /// <summary>
    /// Narrows the framework's view of the originating point to ours, so an implementation declares its
    /// data point once.
    /// </summary>
    IDataPoint IDataPointValue.DataPoint => DataPoint;
}

/// <summary>
/// A value whose payload is typed to <typeparamref name="TDomain"/>, the .NET type its data point
/// exchanges. Only <see cref="LogixDataPoint{TDomain}"/> makes one, and it makes only its own, so a
/// payload cannot disagree with the point it belongs to about its type — which is what lets the write
/// path skip a runtime type check on the value.
/// </summary>
/// <typeparam name="TDomain">The .NET type the data point exchanges — <c>int</c> for a <c>DINT</c>.</typeparam>
internal interface ILogixDataPointValue<out TDomain> : ILogixDataPointValue, ITypedDataPointValue<TDomain>
{
    /// <inheritdoc />
    LogixQuality ILogixDataPointValue.Quality => LogixQuality.Good;

    /// <inheritdoc />
    object? IDataPointValue.Value => TypedValue;
}
