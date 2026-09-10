using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// USINT (0xC6): 8-bit unsigned integer — the same byte as a SINT, told apart only by the declared type,
/// since nothing on the wire records what the top bit means.
/// </summary>
internal sealed class USIntConverter : AtomicDataPointConverter<USIntDataPoint, byte>
{
    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.USInt;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Usint;

    protected override byte DecodeValue(USIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) => buffer[0];

    protected override byte[] EncodeValue(USIntDataPoint dataPoint, byte value) => [value];
}
