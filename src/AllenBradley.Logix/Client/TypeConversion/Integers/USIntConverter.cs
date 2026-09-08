using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

// USINT (0xC6): 8-bit unsigned integer. One byte, so there is no byte order to get wrong — the bit
// pattern is the byte as it stands, and the difference from a SINT is only which half of the range it
// names. That difference is not visible in the bytes, so ExpectedDataType is what keeps a SINT tag from
// being read as this and handing back 255 where the controller holds -1.
internal sealed class USIntConverter : AtomicDataPointConverter<USIntDataPoint, byte>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.USInt;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Usint;

    protected override byte DecodeValue(USIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) => buffer[0];

    protected override byte[] EncodeValue(USIntDataPoint dataPoint, byte value) => [value];
}
