using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// UINT[n] (0xC7, rank 1). Elements are coded by <see cref="UInt16Codec"/>, as the scalar is, so
/// the scalar and array shapes cannot disagree about how wide a UINT is or which way round it sits.
/// </summary>
internal sealed class UIntArrayConverter : AtomicArrayDataPointConverter<UIntArrayDataPoint, ushort>
{
    protected override ElementCodec<ushort> Element => UInt16Codec.Instance;
}
