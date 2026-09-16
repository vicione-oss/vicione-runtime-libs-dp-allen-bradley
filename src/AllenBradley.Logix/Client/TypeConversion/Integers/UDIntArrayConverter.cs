using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// UDINT[n] (0xC8, rank 1). Elements are coded by <see cref="UDIntConverter"/>, so the scalar and array
/// shapes cannot disagree about how wide a UDINT is or which way round it sits.
/// </summary>
internal sealed class UDIntArrayConverter : AtomicArrayDataPointConverter<UDIntArrayDataPoint, uint>
{
    public override AllenBradleyDataType ExpectedDataType => AllenBradleyDataType.Udint;

    protected override int ElementSize => UDIntConverter.ElementSize;

    protected override uint DecodeElement(ReadOnlySpan<byte> buffer) => UDIntConverter.DecodeElement(buffer);

    protected override byte[] EncodeElement(uint element) => UDIntConverter.EncodeElement(element);
}
