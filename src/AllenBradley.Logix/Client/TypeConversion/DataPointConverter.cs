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
internal abstract class DataPointConverter<TDataPoint, TDomain> : IDataPointConverter
    where TDataPoint : LogixDataPoint<TDomain>
{
    public abstract AllenBradleyDataType ExpectedType { get; }

    public abstract ByteSize ByteSize { get; }

    LogixTypeMismatch IDataPointConverter.CompareTo(TagDefinition? metadata) => Compare(metadata);

    bool IDataPointConverter.ConflictsWith(TagDefinition? metadata) =>
        Compare(metadata) != LogixTypeMismatch.None;

    // The one comparison both surface members share. A scalar this converter can decode is a matching
    // atomic type that is neither a structure nor an array. Shape is reported before type: a scalar
    // configured against an array or a structure is a shape mismatch, and its atomic code would be
    // meaningless to compare. Null metadata is unverifiable, not a contradiction, so it reports None.
    private LogixTypeMismatch Compare(TagDefinition? metadata)
    {
        if (metadata is not { } declaration)
        {
            return LogixTypeMismatch.None;
        }

        if (!declaration.DimensionCount.IsScalar)
        {
            return LogixTypeMismatch.Array;
        }

        if (declaration.Kind is LogixTypeKind.Structure)
        {
            return LogixTypeMismatch.Structure;
        }

        return declaration.DataType == ExpectedType
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.AtomicType;
    }

    protected abstract TDomain DecodeValue(TDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    protected abstract void EncodeValue(TDataPoint dataPoint, TDomain value, Span<byte> buffer);

    ILogixDataPointValue IDataPointConverter.Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var typedDataPoint = Cast(dataPoint);
        return typedDataPoint.CreateLogixValue(DecodeValue(typedDataPoint, buffer));
    }

    void IDataPointConverter.Encode(ILogixDataPointValue dataPointValue, Span<byte> buffer)
    {
        var typedDataPoint = Cast(dataPointValue.DataPoint);

        // The payload is asked for through the typed value, not through the framework's object? view:
        // only the data point makes one of these, and it makes only its own, so a value that is not
        // this converter's typed value never held a TDomain to begin with. A Bad value is the ordinary
        // way to arrive here holding nothing, and it is not something to write.
        if (dataPointValue is not ILogixDataPointValue<TDomain> typedValue)
        {
            throw new InvalidOperationException(
                $"Cannot write {dataPointValue.GetType().Name} to {typedDataPoint.TagName}; " +
                $"expected a value carrying {typeof(TDomain).Name}.");
        }

        EncodeValue(typedDataPoint, typedValue.TypedValue, buffer);
    }

    private static TDataPoint Cast(ILogixDataPoint dataPoint) =>
        dataPoint as TDataPoint ?? throw new InvalidOperationException(
            $"{typeof(TDataPoint).Name} converter received {dataPoint.GetType().Name}; " +
            "DataPointConverterRegistry routed the wrong converter.");
}
