using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// USINT[n] (0xC6, rank 1). Elements are coded by <see cref="USIntConverter"/>, so the scalar and array
/// shapes cannot disagree about how wide a USINT is or which way round it sits.
/// </summary>
internal sealed class USIntArrayConverter : AtomicArrayDataPointConverter<USIntArrayDataPoint, byte>
{
    public override AllenBradleyDataType ExpectedDataType => AllenBradleyDataType.Usint;

    protected override int ElementSize => USIntConverter.ElementSize;

    protected override byte DecodeElement(ReadOnlySpan<byte> buffer) => USIntConverter.DecodeElement(buffer);

    protected override byte[] EncodeElement(byte element) => USIntConverter.EncodeElement(element);
}
