using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// UINT (0xC7): 16-bit little-endian unsigned integer — the same two bytes as an INT, told apart only by
/// the declared type, since nothing on the wire records what the top bit means.
/// </summary>
internal sealed class UIntConverter : DataPointConverter<UIntDataPoint, ushort>
{
    private readonly UInt16Codec _codec = new();

    protected override ushort DecodeValue(UIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(UIntDataPoint dataPoint, ushort value) =>
        _codec.Encode(value);
}
