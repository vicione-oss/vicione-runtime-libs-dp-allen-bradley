using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// USINT (0xC6): 8-bit unsigned integer — the same byte as a SINT, told apart only by the declared type,
/// since nothing on the wire records what the top bit means.
/// </summary>
internal sealed class USIntConverter : DataPointConverter<USIntDataPoint, byte>
{
    private readonly ByteCodec _codec = new();

    protected override byte DecodeValue(USIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(USIntDataPoint dataPoint, byte value) =>
        _codec.Encode(value);
}
