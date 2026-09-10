using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// Base for an array of an elementary CIP type: n contiguous elements, no padding, read whole in one
/// request. A subclass supplies the element format alone — how wide one is and how to read it — which
/// keeps that beside the scalar converter for the same type.
/// </summary>
internal abstract class AtomicArrayDataPointConverter<TDataPoint, TElement>
    : AtomicDataPointConverter<TDataPoint, TElement[]>
    where TDataPoint : LogixDataPoint<TElement[]>, ILogixArrayDataPoint
{
    public override DimensionCount ExpectedDimensionCount => DimensionCount.OneDimensional;

    protected abstract int ElementSize { get; }

    protected abstract TElement DecodeElement(ReadOnlySpan<byte> buffer);

    protected sealed override ElementCount? ElementCountOf(TDataPoint dataPoint) => dataPoint.ElementCount;

    protected sealed override TElement[] DecodeValue(TDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var actualElementCount = CountElementsIn(buffer);
        var configuredElementCount = dataPoint.ElementCount.Value;
        if (actualElementCount != configuredElementCount)
        {
            throw new LogixDecodeException(
                $"Cannot read {dataPoint.TagName}; the controller returned {actualElementCount} " +
                $"elements, but the tag is configured with {configuredElementCount}.");
        }

        var elements = new TElement[configuredElementCount];
        for (var index = 0; index < elements.Length; index++)
        {
            elements[index] = DecodeElement(ArrayElementAtIndex(index, buffer));
        }

        return elements;
    }

    // The reply carries elements and nothing else, so its length is how many the controller holds.
    private int CountElementsIn(ReadOnlySpan<byte> buffer) => buffer.Length / ElementSize;

    private ReadOnlySpan<byte> ArrayElementAtIndex(int index, ReadOnlySpan<byte> buffer) =>
        buffer.Slice(index * ElementSize, ElementSize);

    protected override byte[] EncodeValue(TDataPoint dataPoint, TElement[] value) =>
        throw new InvalidOperationException(
            $"Cannot write {dataPoint.TagName}; writing an {ExpectedTypeName} tag is not supported yet.");
}
