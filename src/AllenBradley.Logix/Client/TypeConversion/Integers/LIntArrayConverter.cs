using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// LINT[n] (0xC5, rank 1). Elements are coded by <see cref="Int64Codec"/>, as the scalar is, so
/// the scalar and array shapes cannot disagree about how wide a LINT is or which way round it sits.
/// </summary>
internal sealed class LIntArrayConverter : AtomicArrayDataPointConverter<LIntArrayDataPoint, long>
{
    protected override ElementCodec<long> Element { get; } = new Int64Codec();
}
