using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// Base for an array of an elementary CIP type: n contiguous elements, no padding, transferred whole in
/// one request. A subclass supplies the element format alone — how wide one is, how to read it and how
/// to write it — which keeps that beside the scalar converter for the same type.
/// </summary>
internal abstract class AtomicArrayDataPointConverter<TDataPoint, TElement>
    : DataPointConverter<TDataPoint, TElement[]>
    where TDataPoint : LogixDataPoint<TElement[]>, ILogixArrayDataPoint
{
    protected abstract int ElementSize { get; }

    protected abstract TElement DecodeElement(ReadOnlySpan<byte> buffer);

    protected abstract byte[] EncodeElement(TElement element);

    protected sealed override TElement[] DecodeValue(TDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var configuredElementCount = dataPoint.ElementCount.Value;

        // The reply carries elements and nothing else, so the configured count fixes its length exactly.
        // Compared in bytes rather than in elements: a length that is not a whole number of them is the
        // same disagreement, and dividing it away would decode the elements that did fit.
        if (buffer.Length != configuredElementCount * ElementSize)
        {
            throw new LogixDecodeException(
                $"Cannot read {TagNameOf(dataPoint)}; the controller returned {buffer.Length} bytes, " +
                $"but the tag is configured with {configuredElementCount} elements of {ElementSize} bytes.");
        }

        var elements = new TElement[configuredElementCount];
        for (var index = 0; index < elements.Length; index++)
        {
            elements[index] = DecodeElement(ArrayElementAtIndex(index, buffer));
        }

        return elements;
    }

    private ReadOnlySpan<byte> ArrayElementAtIndex(int index, ReadOnlySpan<byte> buffer) =>
        buffer.Slice(index * ElementSize, ElementSize);

    protected sealed override byte[] EncodeValue(TDataPoint dataPoint, TElement[] value)
    {
        var configuredElementCount = dataPoint.ElementCount.Value;
        if (value.Length != configuredElementCount)
        {
            // Refused here rather than sent short: SetBuffer would fill the handle from the start and
            // leave the tail of the tag as the controller had it, which is a partial write in disguise.
            throw new InvalidOperationException(
                $"Cannot write {TagNameOf(dataPoint)}; the value holds {value.Length} elements, " +
                $"but the tag is configured with {configuredElementCount}.");
        }

        var buffer = new byte[configuredElementCount * ElementSize];
        for (var index = 0; index < value.Length; index++)
        {
            var encodedElement = EncodeElement(value[index]);
            encodedElement.CopyTo(buffer, index * ElementSize);
        }

        return buffer;
    }

    // The type parameter is both a data point and an array data point, and each names the tag.
    private static TagAddress TagNameOf(TDataPoint dataPoint) => ((ILogixDataPoint)dataPoint).TagAddress;
}
