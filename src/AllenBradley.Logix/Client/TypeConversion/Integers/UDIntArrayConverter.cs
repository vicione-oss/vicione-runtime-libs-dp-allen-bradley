using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// UDINT[n] (0xC8, rank 1). Elements are coded by <see cref="UInt32Codec"/>, as the scalar is, so
/// the scalar and array shapes cannot disagree about how wide a UDINT is or which way round it sits.
/// </summary>
internal sealed class UDIntArrayConverter : AtomicArrayDataPointConverter<UDIntArrayDataPoint, uint>
{
    protected override ElementCodec<uint> Element => UInt32Codec.Instance;
}
