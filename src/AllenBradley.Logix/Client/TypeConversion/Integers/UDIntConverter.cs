using ViciOne.Suite.DataPort.AllenBradley.LibPlcTag.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// UDINT (0xC8): 32-bit little-endian unsigned integer — the same four bytes as a DINT, told apart only by
/// the declared type, since nothing on the wire records what the top bit means.
/// </summary>
internal sealed class UDIntConverter : DataPointConverter<UDIntDataPoint, uint>
{
    private readonly UInt32Codec _codec = new();

    protected override uint DecodeValue(UDIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(UDIntDataPoint dataPoint, uint value) =>
        _codec.Encode(value);
}
