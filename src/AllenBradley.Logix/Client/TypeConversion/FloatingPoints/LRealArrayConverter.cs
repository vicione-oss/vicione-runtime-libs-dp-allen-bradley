using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

/// <summary>
/// LREAL[n] (0xCB, rank 1). Elements are coded by <see cref="DoubleCodec"/>, as the scalar is, so
/// the scalar and array shapes cannot disagree about how wide an LREAL is or which way round it sits.
/// </summary>
internal sealed class LRealArrayConverter : AtomicArrayDataPointConverter<LRealArrayDataPoint, double>
{
    protected override ElementCodec<double> Element => DoubleCodec.Instance;
}
