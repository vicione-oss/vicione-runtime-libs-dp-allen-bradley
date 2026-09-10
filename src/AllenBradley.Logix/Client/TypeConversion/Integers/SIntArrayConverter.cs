using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// SINT[n] (0xC2, rank 1). Elements are coded by <see cref="SIntConverter"/>, so the scalar and array
/// shapes cannot disagree about how wide a SINT is or which way round it sits.
/// </summary>
internal sealed class SIntArrayConverter : AtomicArrayDataPointConverter<SIntArrayDataPoint, sbyte>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.SIntArray;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Sint;

    protected override int ElementSize => SIntConverter.ElementSize;

    protected override sbyte DecodeElement(ReadOnlySpan<byte> buffer) => SIntConverter.DecodeElement(buffer);

    protected override byte[] EncodeElement(sbyte element) => SIntConverter.EncodeElement(element);
}
