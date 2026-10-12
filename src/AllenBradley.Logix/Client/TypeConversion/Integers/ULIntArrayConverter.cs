using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// ULINT[n] (0xC9, rank 1). Elements are coded by <see cref="UInt64Codec"/>, as the scalar is, so
/// the scalar and array shapes cannot disagree about how wide a ULINT is or which way round it sits.
/// </summary>
internal sealed class ULIntArrayConverter : AtomicArrayDataPointConverter<ULIntArrayDataPoint, ulong>
{
    protected override ElementCodec<ulong> Element => UInt64Codec.Instance;
}
