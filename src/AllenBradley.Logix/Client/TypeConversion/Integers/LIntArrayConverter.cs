using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// LINT[n] (0xC5, rank 1). Elements are coded by <see cref="LIntConverter"/>, so the scalar and array
/// shapes cannot disagree about how wide a LINT is or which way round it sits.
/// </summary>
internal sealed class LIntArrayConverter : AtomicArrayDataPointConverter<LIntArrayDataPoint, long>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.LIntArray;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Lint;

    protected override int ElementSize => LIntConverter.ElementSize;

    protected override long DecodeElement(ReadOnlySpan<byte> buffer) => LIntConverter.DecodeElement(buffer);

    protected override byte[] EncodeElement(long element) => LIntConverter.EncodeElement(element);
}
