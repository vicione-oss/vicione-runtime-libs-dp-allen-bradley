using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

// INT[n] (0xC3, rank 1). Elements are decoded by IntConverter, so nothing here knows how wide an INT is
// or which way round it sits — the one thing the scalar and array shapes could otherwise disagree about.
internal sealed class IntArrayConverter : AtomicArrayDataPointConverter<IntArrayDataPoint, short>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.IntArray;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Int;

    protected override int ElementSize => IntConverter.ElementSize;

    protected override short DecodeElement(ReadOnlySpan<byte> buffer) => IntConverter.DecodeElement(buffer);
}
