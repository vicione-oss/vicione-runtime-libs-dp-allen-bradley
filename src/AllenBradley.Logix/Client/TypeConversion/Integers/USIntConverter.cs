using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// USINT (0xC6): 8-bit unsigned integer — the same byte as a SINT, told apart only by the declared type,
/// since nothing on the wire records what the top bit means.
/// </summary>
internal sealed class USIntConverter : DataPointConverter<USIntDataPoint, byte>
{
    protected override byte DecodeValue(USIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        ByteCodec.Instance.Decode(buffer);

    protected override byte[] EncodeValue(USIntDataPoint dataPoint, byte value) =>
        ByteCodec.Instance.Encode(value);
}
