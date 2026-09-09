using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

// INT[n] (0xC3, rank 1): n contiguous INTs, no padding between them, so element i is the scalar codec at
// offset i × 2. One handle, one request, one value — the array is read whole, never per element.
internal sealed class IntArrayConverter : AtomicDataPointConverter<IntArrayDataPoint, short[]>
{
    private const int ElementSize = sizeof(short);

    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.IntArray;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Int;

    protected override short[] DecodeValue(IntArrayDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var elements = new short[dataPoint.ElementCount.Value];

        // The whole extent is taken first, so a reply too short for the declared count is one refusal
        // that LogixReadBatch names the tag in, rather than an array filled as far as the bytes went.
        var storedElements = buffer.Slice(0, elements.Length * ElementSize);
        for (var index = 0; index < elements.Length; index++)
        {
            elements[index] = BinaryPrimitives.ReadInt16LittleEndian(storedElements[(index * ElementSize)..]);
        }

        return elements;
    }

    // Writing an array is its own slice, and it brings the question of what a partial write means. Said
    // as a refusal the write batch collects and names the tag in, the way an over-long STRING is said.
    protected override byte[] EncodeValue(IntArrayDataPoint dataPoint, short[] value) =>
        throw new InvalidOperationException(
            $"Cannot write {dataPoint.TagName}; writing an {ExpectedTypeName} tag is not supported yet.");
}
