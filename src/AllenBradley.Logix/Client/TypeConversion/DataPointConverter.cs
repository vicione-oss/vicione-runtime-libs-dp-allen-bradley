using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// Typed base for IDataPointConverter. Subclasses work entirely in their own data-point and .NET types
// (DIntDataPoint <-> int) and never touch object or ILogixDataPoint. The single runtime boundary cast
// lives here; a failed cast can only mean the registry routed the wrong converter, and it is reported
// as exactly that, in one place.
internal abstract class DataPointConverter<TDataPoint, TDomain> : IDataPointConverter
    where TDataPoint : class, ILogixDataPoint
{
    public abstract CipType ExpectedType { get; }

    public abstract ByteSize ByteSize { get; }

    LogixTypeMismatch IDataPointConverter.CompareTo(LogixTypeDeclaration? metadata) => Compare(metadata);

    bool IDataPointConverter.ConflictsWith(LogixTypeDeclaration? metadata) =>
        Compare(metadata) != LogixTypeMismatch.None;

    // The one comparison both surface members share. A scalar this converter can decode is a matching
    // atomic type that is neither a structure nor an array. Shape is reported before type: a scalar
    // configured against an array or a structure is a shape mismatch, and its atomic code would be
    // meaningless to compare. Null metadata is unverifiable, not a contradiction, so it reports None.
    private LogixTypeMismatch Compare(LogixTypeDeclaration? metadata)
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

        return declaration.AtomicType == ExpectedType
            ? LogixTypeMismatch.None
            : LogixTypeMismatch.AtomicType;
    }

    protected abstract TDomain DecodeValue(TDataPoint dataPoint, ReadOnlySpan<byte> buffer);

    protected abstract void EncodeValue(TDataPoint dataPoint, TDomain value, Span<byte> buffer);

    ILogixDataPointValue IDataPointConverter.Decode(ILogixDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var typedDataPoint = Cast(dataPoint);
        var value = DecodeValue(typedDataPoint, buffer);
        return new LogixDataPointValue<TDomain>(typedDataPoint, value, LogixQuality.Good);
    }

    ILogixDataPointValue IDataPointConverter.CreateBadValue(ILogixDataPoint dataPoint) =>
        new LogixDataPointValue<TDomain>(Cast(dataPoint), default, LogixQuality.Bad);

    void IDataPointConverter.Encode(ILogixDataPointValue dataPointValue, Span<byte> buffer)
    {
        var typedDataPoint = Cast(dataPointValue.DataPoint);
        if (dataPointValue.Value is not TDomain value)
        {
            throw new InvalidOperationException(
                $"Cannot write {dataPointValue.Value?.GetType().Name ?? "null"} to {typedDataPoint.TagName}; " +
                $"expected {typeof(TDomain).Name}.");
        }

        EncodeValue(typedDataPoint, value, buffer);
    }

    private static TDataPoint Cast(ILogixDataPoint dataPoint) =>
        dataPoint as TDataPoint ?? throw new InvalidOperationException(
            $"{typeof(TDataPoint).Name} converter received {dataPoint.GetType().Name}; " +
            "DataPointConverterRegistry routed the wrong converter.");
}
