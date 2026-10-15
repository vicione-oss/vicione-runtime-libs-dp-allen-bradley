using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>Converts an element of an integer (<c>N</c>) file, one 16-bit word, to a <see cref="short"/> and back.</summary>
internal sealed class IntegerConverter : LegacyDataPointConverter<IntegerDataPoint, short>
{
    private readonly Int16Codec _codec = new();

    protected override short DecodeValue(IntegerDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(IntegerDataPoint dataPoint, short value) => _codec.Encode(value);
}
