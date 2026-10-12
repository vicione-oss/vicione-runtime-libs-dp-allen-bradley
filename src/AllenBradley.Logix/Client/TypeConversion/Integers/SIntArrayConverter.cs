using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// SINT[n] (0xC2, rank 1). Elements are coded by <see cref="SByteCodec"/>, as the scalar is, so
/// the scalar and array shapes cannot disagree about how wide a SINT is or which way round it sits.
/// </summary>
internal sealed class SIntArrayConverter : AtomicArrayDataPointConverter<SIntArrayDataPoint, sbyte>
{
    protected override ElementCodec<sbyte> Element { get; } = new SByteCodec();
}
