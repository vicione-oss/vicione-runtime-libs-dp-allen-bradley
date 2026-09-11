using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Booleans;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Booleans;

/// <summary>
/// BOOL[n] (0xD3, rank 1) is a special array-case as it is not n contiguous elements. Therefore, it does not extend the
/// base array-convert but is handled separately.
/// The buffer is n words into which the bits/bools are packed into, and element i is a bit of one rather than a slice of bytes.
/// </summary>
internal sealed class BoolArrayConverter : AtomicDataPointConverter<BoolArrayDataPoint, bool[]>
{
    private const int BitsPerByte = 8;

    private const int BytesPerWord = BoolArrayDataPoint.BoolsPerWord / BitsPerByte;

    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.BoolArray;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Bool;

    public override DimensionCount ExpectedDimensionCount => DimensionCount.OneDimensional;

    protected override ElementCount? ElementCountOf(BoolArrayDataPoint dataPoint) => dataPoint.ElementCount;

    protected override bool[] DecodeValue(BoolArrayDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        ThrowIfNotTheDeclaredWidth(dataPoint, buffer.Length);

        var bits = new bool[dataPoint.ElementCount.Value];
        for (var index = 0; index < bits.Length; index++)
        {
            bits[index] = (buffer[index / BitsPerByte] & MaskOf(index)) != 0;
        }

        return bits;
    }

    protected override byte[] EncodeValue(BoolArrayDataPoint dataPoint, bool[] value)
    {
        var declaredBitCount = dataPoint.ElementCount.Value;
        if (value.Length != declaredBitCount)
        {
            // Refused here rather than sent short: SetBuffer would fill the handle from the start and
            // leave the tail of the tag as the controller had it, which is a partial write in disguise.
            throw new InvalidOperationException(
                $"Cannot write {dataPoint.TagName}; the value holds {value.Length} elements, " +
                $"but the tag is configured with {declaredBitCount}.");
        }

        // Whole words, zero-filled: Studio 5000 declares no BOOL array whose bits do not fill them, so a
        // bit this tag does not own is a bit no tag owns, and writing it clears nothing that is in use.
        var buffer = new byte[WidthInBytes(dataPoint)];
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index])
            {
                buffer[index / BitsPerByte] |= MaskOf(index);
            }
        }

        return buffer;
    }

    // Compared against the words the reply carries rather than against the bit count: a BOOL array is
    // transferred whole words at a time, so a buffer short of one is a reply about a different tag.
    private static void ThrowIfNotTheDeclaredWidth(BoolArrayDataPoint dataPoint, int bufferLength)
    {
        var declaredWidth = WidthInBytes(dataPoint);
        if (bufferLength != declaredWidth)
        {
            throw new LogixDecodeException(
                $"Cannot read {dataPoint.TagName}; the controller returned {bufferLength} bytes, " +
                $"but the tag is configured with {dataPoint.ElementCount.Value} elements packed into " +
                $"{declaredWidth} bytes.");
        }
    }

    private static int WidthInBytes(BoolArrayDataPoint dataPoint) =>
        dataPoint.WordCount.Value * BytesPerWord;

    // Little-endian throughout: bit i is bit i % 8 of byte i / 8, so bit 32 opens the second word.
    private static byte MaskOf(int index) => (byte)(1 << (index % BitsPerByte));
}
