using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// Typed base for the Logix <see cref="IDataPointConverter{TDataPoint, TValue}"/>. Subclasses work entirely in their own data-point and
/// .NET types (DIntDataPoint to int) and never touch <c>object</c>; the single boundary cast lives here, so
/// a failed cast can only mean the registry routed the wrong converter.
/// </summary>
internal abstract class DataPointConverter<TDataPoint, TDomain>
    : IDataPointConverter<ILogixDataPoint, ILogixDataPointValue>
    where TDataPoint : LogixDataPoint<TDomain>
{
    protected abstract TDomain DecodeValue(TDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// The freshly allocated bytes the value occupies, sized from the type or the data point's
    /// configuration — never from a tag.
    /// </summary>
    protected abstract byte[] EncodeValue(TDataPoint dataPoint, TDomain value);

    ILogixDataPointValue IDataPointConverter<ILogixDataPoint, ILogixDataPointValue>.Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var typedDataPoint = Cast(dataPoint);
        return typedDataPoint.CreateLogixValue(DecodeValue(typedDataPoint, buffer));
    }

    byte[] IDataPointConverter<ILogixDataPoint, ILogixDataPointValue>.Encode(ILogixDataPointValue dataPointValue)
    {
        var typedDataPoint = Cast(dataPointValue.DataPoint);

        // A data point makes only its own value, so ILogixDataPointValue being public is the one way a
        // foreign value reaches here.
        if (dataPointValue is not ILogixDataPointValue<TDomain> typedValue)
        {
            throw new InvalidOperationException(
                $"Cannot write {dataPointValue.GetType().Name} to {typedDataPoint.TagAddress}; " +
                $"expected a value carrying {typeof(TDomain).Name}.");
        }

        return EncodeValue(typedDataPoint, typedValue.TypedValue);
    }

    private static TDataPoint Cast(ILogixDataPoint dataPoint) =>
        dataPoint as TDataPoint ?? throw new InvalidOperationException(
            $"{typeof(TDataPoint).Name} converter received {dataPoint.GetType().Name}; " +
            "DataPointConverterRegistry routed the wrong converter.");
}
