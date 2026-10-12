using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// DINT[n] (0xC4, rank 1). Elements are coded by <see cref="Int32Codec"/>, as the scalar is, so
/// the scalar and array shapes cannot disagree about how wide a DINT is or which way round it sits.
/// </summary>
internal sealed class DIntArrayConverter : AtomicArrayDataPointConverter<DIntArrayDataPoint, int>
{
    protected override ElementCodec<int> Element { get; } = new Int32Codec();
}
