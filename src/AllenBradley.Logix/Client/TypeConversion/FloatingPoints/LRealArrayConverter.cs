using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

/// <summary>
/// LREAL[n] (0xCB, rank 1). Elements are coded by <see cref="LRealConverter"/>, so the scalar and array
/// shapes cannot disagree about how wide an LREAL is or which way round it sits.
/// </summary>
internal sealed class LRealArrayConverter : AtomicArrayDataPointConverter<LRealArrayDataPoint, double>
{
    protected override int ElementSize => LRealConverter.ElementSize;

    protected override double DecodeElement(ReadOnlySpan<byte> buffer) => LRealConverter.DecodeElement(buffer);

    protected override byte[] EncodeElement(double element) => LRealConverter.EncodeElement(element);
}
