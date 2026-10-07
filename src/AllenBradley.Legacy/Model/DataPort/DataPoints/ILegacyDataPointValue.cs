using ViciOne.Suite.DataPort.Extensions.Model.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

/// <summary>
/// The result of reading, or the payload for writing, one data point: the originating point and the
/// value it carries.
/// </summary>
public interface ILegacyDataPointValue : IDataPointValue
{
    new ILegacyDataPoint DataPoint { get; }

    /// <summary>
    /// Narrows the framework's view of the originating point to ours, so an implementation declares its
    /// data point once.
    /// </summary>
    IDataPoint IDataPointValue.DataPoint => DataPoint;
}

/// <summary>
/// A value whose payload is typed to <typeparamref name="TDomain"/>, the .NET type its data point
/// exchanges. Only <see cref="LegacyDataPoint{TDomain}"/> makes one, and only its own, so a payload cannot
/// disagree with its point about its type.
/// </summary>
/// <typeparam name="TDomain">The .NET type the data point exchanges — <c>short</c> for an integer file.</typeparam>
internal interface ILegacyDataPointValue<out TDomain> : ILegacyDataPointValue, ITypedDataPointValue<TDomain>
{
    /// <inheritdoc />
    object? IDataPointValue.Value => TypedValue;
}
