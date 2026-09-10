using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Arrays.FloatingPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.FloatingPoints;

/// <summary>
/// REAL[n] (0xCA, rank 1). Elements are coded by <see cref="RealConverter"/>, so the scalar and array
/// shapes cannot disagree about how wide a REAL is or which way round it sits.
/// </summary>
internal sealed class RealArrayConverter : AtomicArrayDataPointConverter<RealArrayDataPoint, float>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.RealArray;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Real;

    protected override int ElementSize => RealConverter.ElementSize;

    protected override float DecodeElement(ReadOnlySpan<byte> buffer) => RealConverter.DecodeElement(buffer);

    protected override byte[] EncodeElement(float element) => RealConverter.EncodeElement(element);
}
