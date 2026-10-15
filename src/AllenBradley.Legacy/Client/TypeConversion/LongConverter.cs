using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>Converts an element of a long (<c>L</c>) file, one 32-bit integer, to an <see cref="int"/> and back.</summary>
internal sealed class LongConverter : LegacyDataPointConverter<LongDataPoint, int>
{
    private readonly Int32Codec _codec = new();

    protected override int DecodeValue(LongDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(LongDataPoint dataPoint, int value) => _codec.Encode(value);
}
