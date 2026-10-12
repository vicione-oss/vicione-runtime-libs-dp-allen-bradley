using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

/// <summary>
/// Base for an array of an elementary CIP type: n contiguous elements, no padding, transferred whole in
/// one request. A subclass supplies the element codec alone, the same one the scalar converter for the
/// type uses, so the two shapes cannot disagree about how wide an element is or which way round it sits.
/// </summary>
internal abstract class AtomicArrayDataPointConverter<TDataPoint, TElement>
    : DataPointConverter<TDataPoint, TElement[]>
    where TDataPoint : LogixDataPoint<TElement[]>, ILogixArrayDataPoint
{
    protected abstract ElementCodec<TElement> Element { get; }

    protected sealed override TElement[] DecodeValue(TDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var codec = CodecFor(dataPoint);

        // The reply carries elements and nothing else, so the configured count fixes its length exactly.
        // Compared in bytes rather than in elements: a length that is not a whole number of them is the
        // same disagreement, and dividing it away would decode the elements that did fit.
        if (buffer.Length != codec.Size)
        {
            throw new DecodeException(
                $"Cannot read {TagNameOf(dataPoint)}; the controller returned {buffer.Length} bytes, " +
                $"but the tag is configured with {dataPoint.ElementCount.Value} elements of {Element.Size} bytes.");
        }

        return codec.Decode(buffer);
    }

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

        return CodecFor(dataPoint).Encode(value);
    }

    private ArrayCodec<TElement> CodecFor(TDataPoint dataPoint) =>
        new(Element, (int)dataPoint.ElementCount.Value);

    // The type parameter is both a data point and an array data point, and each names the tag.
    private static TagAddress TagNameOf(TDataPoint dataPoint) => ((ILogixDataPoint)dataPoint).TagAddress;
}
