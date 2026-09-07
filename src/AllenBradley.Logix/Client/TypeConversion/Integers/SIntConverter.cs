using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

// SINT (0xC2): 8-bit signed integer. One byte, so there is no byte order to get wrong — the bit pattern
// is the two's-complement sbyte as it stands.
internal sealed class SIntConverter : AtomicDataPointConverter<SIntDataPoint, sbyte>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.SInt;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Sint;

    protected override sbyte DecodeValue(SIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) => (sbyte)buffer[0];

    protected override byte[] EncodeValue(SIntDataPoint dataPoint, sbyte value) => [(byte)value];
}
