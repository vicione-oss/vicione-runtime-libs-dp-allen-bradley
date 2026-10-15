using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Floats;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>Converts an element of a float (<c>F</c>) file, one IEEE 754 single, to a <see cref="float"/> and back.</summary>
internal sealed class FloatConverter : LegacyDataPointConverter<FloatDataPoint, float>
{
    private readonly SingleCodec _codec = new();

    protected override float DecodeValue(FloatDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(FloatDataPoint dataPoint, float value) => _codec.Encode(value);
}
