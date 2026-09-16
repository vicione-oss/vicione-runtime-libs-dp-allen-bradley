using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// SINT (0xC2): 8-bit signed integer. One byte, so there is no byte order to get wrong — the bit pattern is
/// the two's-complement sbyte as it stands.
/// </summary>
internal sealed class SIntConverter : AtomicDataPointConverter<SIntDataPoint, sbyte>
{
    internal const int ElementSize = sizeof(sbyte);

    public override AllenBradleyDataType ExpectedDataType => AllenBradleyDataType.Sint;

    internal static sbyte DecodeElement(ReadOnlySpan<byte> buffer) => (sbyte)buffer[0];

    internal static byte[] EncodeElement(sbyte value) => [(byte)value];

    protected override sbyte DecodeValue(SIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DecodeElement(buffer);

    protected override byte[] EncodeValue(SIntDataPoint dataPoint, sbyte value) =>
        EncodeElement(value);
}
