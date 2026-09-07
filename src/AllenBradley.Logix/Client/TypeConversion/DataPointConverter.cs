using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// Typed base for IDataPointConverter. Subclasses work entirely in their own data-point and .NET types
// (DIntDataPoint <-> int) and never touch object or ILogixDataPoint. The single runtime boundary cast
// lives here; a failed cast can only mean the registry routed the wrong converter, and it is reported
// as exactly that, in one place.
//
// The data point is a LogixDataPoint<TDomain> rather than any ILogixDataPoint, so the pairing of a
// converter with the .NET type it decodes is checked by the compiler: a converter cannot be written
// against a data point that exchanges something else. It is also what lets Decode build the value
// through the data point instead of a value type of its own — the point owns its value record, and the
// range rules that record carries.
//
// What a match *is* is not decided here or in any subclass. A converter states what it expects the tag
// to be — ExpectedKind, ExpectedDataType and MaxLengthOf — and LogixTypeComparison holds the one rule
// that reads a TagDefinition against it. An elementary type and a STRING are then two sets of constants
// rather than two comparisons that must agree.
internal abstract class DataPointConverter<TDataPoint, TDomain> : IDataPointConverter
    where TDataPoint : LogixDataPoint<TDomain>
{
    public abstract LogixDataTypeName ExpectedTypeName { get; }

    public abstract LogixTypeKind ExpectedKind { get; }

    public abstract AllenBradleyDataType? ExpectedDataType { get; }

    StringMaxLength? IDataPointConverter.MaxLengthFor(ILogixDataPoint dataPoint) =>
        MaxLengthOf(Cast(dataPoint));

    // The capacity the controller must declare for this data point, or null when the type fixes its own
    // size and there is nothing left to agree on.
    protected abstract StringMaxLength? MaxLengthOf(TDataPoint dataPoint);

    protected abstract TDomain DecodeValue(TDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    // Returns the bytes the value occupies on the wire, freshly allocated: a subclass sizes them from
    // its type or the data point's configuration, never from a tag.
    protected abstract byte[] EncodeValue(TDataPoint dataPoint, TDomain value);

    ILogixDataPointValue IDataPointConverter.Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var typedDataPoint = Cast(dataPoint);
        return typedDataPoint.CreateLogixValue(DecodeValue(typedDataPoint, buffer));
    }

    byte[] IDataPointConverter.Encode(ILogixDataPointValue dataPointValue)
    {
        var typedDataPoint = Cast(dataPointValue.DataPoint);

        // The payload is asked for through the typed value, not through the framework's object? view:
        // only the data point makes one of these, and it makes only its own, so a value that is not
        // this converter's typed value never held a TDomain to begin with. ILogixDataPointValue is
        // public, so an outside implementation is the one way one can arrive here.
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
