using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// DINT[n] (0xC4, rank 1). Elements are coded by <see cref="DIntConverter"/>, so the scalar and array
/// shapes cannot disagree about how wide a DINT is or which way round it sits.
/// </summary>
internal sealed class DIntArrayConverter : AtomicArrayDataPointConverter<DIntArrayDataPoint, int>
{
    protected override int ElementSize => DIntConverter.ElementSize;

    protected override int DecodeElement(ReadOnlySpan<byte> buffer) => DIntConverter.DecodeElement(buffer);

    protected override byte[] EncodeElement(int element) => DIntConverter.EncodeElement(element);
}
