using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>
/// Typed base for the Legacy <see cref="IDataPointConverter{TDataPoint, TValue}"/>. A subclass works in its own
/// data point and .NET type and never uses <c>object</c>. The one cast from the interface type is here, so a
/// failed cast can only mean that <see cref="LegacyConverters"/> routed the wrong converter.
/// </summary>
internal abstract class LegacyDataPointConverter<TDataPoint, TDomain>
    : IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue>
    where TDataPoint : LegacyDataPoint<TDomain>
{
    protected abstract TDomain DecodeValue(TDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    /// <summary>The new bytes that the value occupies. The size comes from the type, not from a tag.</summary>
    protected abstract byte[] EncodeValue(TDataPoint dataPoint, TDomain value);

    ILegacyDataPointValue IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue>.Decode(
        ILegacyDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var typedDataPoint = Cast(dataPoint);
        return typedDataPoint.CreateLegacyValue(DecodeValue(typedDataPoint, buffer));
    }

    byte[] IDataPointConverter<ILegacyDataPoint, ILegacyDataPointValue>.Encode(ILegacyDataPointValue value)
    {
        var typedDataPoint = Cast(value.DataPoint);

        // Only a data point creates its values, so a value of another type can only come from outside the dataport.
        if (value is not ILegacyDataPointValue<TDomain> typedValue)
        {
            throw new InvalidOperationException(
                $"Cannot write {value.GetType().Name} to {typedDataPoint.Address}; " +
                $"expected a value that carries {typeof(TDomain).Name}.");
        }

        return EncodeValue(typedDataPoint, typedValue.TypedValue);
    }

    private static TDataPoint Cast(ILegacyDataPoint dataPoint) =>
        dataPoint as TDataPoint ?? throw new InvalidOperationException(
            $"The {typeof(TDataPoint).Name} converter received {dataPoint.GetType().Name}; " +
            "LegacyConverters routed the wrong converter.");
}
