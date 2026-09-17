using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// INT[n] (0xC3, rank 1). Elements are coded by <see cref="IntConverter"/>, so the scalar and array
/// shapes cannot disagree about how wide an INT is or which way round it sits.
/// </summary>
internal sealed class IntArrayConverter : AtomicArrayDataPointConverter<IntArrayDataPoint, short>
{
    protected override int ElementSize => IntConverter.ElementSize;

    protected override short DecodeElement(ReadOnlySpan<byte> buffer) => IntConverter.DecodeElement(buffer);

    protected override byte[] EncodeElement(short element) => IntConverter.EncodeElement(element);
}
