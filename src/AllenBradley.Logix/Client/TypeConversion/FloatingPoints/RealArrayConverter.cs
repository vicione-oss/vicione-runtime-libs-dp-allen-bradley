using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

/// <summary>
/// REAL[n] (0xCA, rank 1). Elements are coded by <see cref="SingleCodec"/>, as the scalar is, so
/// the scalar and array shapes cannot disagree about how wide a REAL is or which way round it sits.
/// </summary>
internal sealed class RealArrayConverter : AtomicArrayDataPointConverter<RealArrayDataPoint, float>
{
    protected override ElementCodec<float> Element { get; } = new SingleCodec();
}
