using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// UINT[n] (0xC7, rank 1). Elements are coded by <see cref="UIntConverter"/>, so the scalar and array
/// shapes cannot disagree about how wide a UINT is or which way round it sits.
/// </summary>
internal sealed class UIntArrayConverter : AtomicArrayDataPointConverter<UIntArrayDataPoint, ushort>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.UIntArray;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Uint;

    protected override int ElementSize => UIntConverter.ElementSize;

    protected override ushort DecodeElement(ReadOnlySpan<byte> buffer) => UIntConverter.DecodeElement(buffer);

    protected override byte[] EncodeElement(ushort element) => UIntConverter.EncodeElement(element);
}
