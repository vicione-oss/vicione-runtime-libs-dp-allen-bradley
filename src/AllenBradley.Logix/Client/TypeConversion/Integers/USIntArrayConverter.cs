using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// USINT[n] (0xC6, rank 1). Elements are coded by <see cref="ByteCodec"/>, as the scalar is, so
/// the scalar and array shapes cannot disagree about how wide a USINT is or which way round it sits.
/// </summary>
internal sealed class USIntArrayConverter : AtomicArrayDataPointConverter<USIntArrayDataPoint, byte>
{
    protected override ElementCodec<byte> Element { get; } = new ByteCodec();
}
