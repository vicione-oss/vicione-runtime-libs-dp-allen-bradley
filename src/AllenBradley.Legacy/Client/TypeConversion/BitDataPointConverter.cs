using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Binary;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>
/// Converts one bit of a binary (<c>B</c>) file element to a <see cref="bool"/> and back. The bit tag access
/// carries the bit as one byte, so the converter never sees the other 15 bits of the element.
/// </summary>
internal sealed class BitDataPointConverter : LegacyDataPointConverter<BitDataPoint, bool>
{
    private readonly BitCodec _codec = new();

    protected override bool DecodeValue(BitDataPoint dataPoint, ReadOnlySpan<byte> buffer) => _codec.Decode(buffer);

    protected override byte[] EncodeValue(BitDataPoint dataPoint, bool value) => _codec.Encode(value);
}
