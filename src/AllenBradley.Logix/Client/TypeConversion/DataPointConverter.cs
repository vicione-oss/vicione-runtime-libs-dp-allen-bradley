using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// Typed base for <see cref="IDataPointConverter"/>. Subclasses work entirely in their own data-point and
/// .NET types (DIntDataPoint to int) and never touch <c>object</c>; the single boundary cast lives here, so
/// a failed cast can only mean the registry routed the wrong converter.
/// </summary>
internal abstract class DataPointConverter<TDataPoint, TDomain> : IDataPointConverter
    where TDataPoint : LogixDataPoint<TDomain>
{
    public abstract LogixTypeKind ExpectedKind { get; }

    public abstract AllenBradleyDataType ExpectedDataType { get; }

    public virtual DimensionCount ExpectedDimensionCount => DimensionCount.Scalar;

    StringMaxLength? IDataPointConverter.MaxLengthFor(ILogixDataPoint dataPoint) =>
        MaxLengthOf(Cast(dataPoint));

    ElementCount? IDataPointConverter.ElementCountFor(ILogixDataPoint dataPoint) =>
        ElementCountOf(Cast(dataPoint));

    /// <summary>
    /// The character capacity the controller must declare, or null when the type fixes its own size.
    /// </summary>
    protected abstract StringMaxLength? MaxLengthOf(TDataPoint dataPoint);

    /// <summary>
    /// How many elements the controller must declare, or null when the shape holds a single value.
    /// </summary>
    protected virtual ElementCount? ElementCountOf(TDataPoint dataPoint) => null;

    protected abstract TDomain DecodeValue(TDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// The freshly allocated bytes the value occupies, sized from the type or the data point's
    /// configuration — never from a tag.
    /// </summary>
    protected abstract byte[] EncodeValue(TDataPoint dataPoint, TDomain value);

    ILogixDataPointValue IDataPointConverter.Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var typedDataPoint = Cast(dataPoint);
        return typedDataPoint.CreateLogixValue(DecodeValue(typedDataPoint, buffer));
    }

    byte[] IDataPointConverter.Encode(ILogixDataPointValue dataPointValue)
    {
        var typedDataPoint = Cast(dataPointValue.DataPoint);

        // A data point makes only its own value, so ILogixDataPointValue being public is the one way a
        // foreign value reaches here.
        if (dataPointValue is not ILogixDataPointValue<TDomain> typedValue)
        {
            throw new InvalidOperationException(
                $"Cannot write {dataPointValue.GetType().Name} to {typedDataPoint.TagName}; " +
                $"expected a value carrying {typeof(TDomain).Name}.");
        }

        return EncodeValue(typedDataPoint, typedValue.TypedValue);
    }

    private static TDataPoint Cast(ILogixDataPoint dataPoint) =>
        dataPoint as TDataPoint ?? throw new InvalidOperationException(
            $"{typeof(TDataPoint).Name} converter received {dataPoint.GetType().Name}; " +
            "DataPointConverterRegistry routed the wrong converter.");
}
