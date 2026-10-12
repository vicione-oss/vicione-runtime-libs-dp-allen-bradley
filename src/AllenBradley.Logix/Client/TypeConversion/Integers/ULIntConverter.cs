using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// ULINT (0xC9): 64-bit little-endian unsigned integer — the same eight bytes as a LINT, told apart only by
/// the declared type, since nothing on the wire records what the top bit means.
/// </summary>
internal sealed class ULIntConverter : DataPointConverter<ULIntDataPoint, ulong>
{
    private readonly UInt64Codec _codec = new();

    protected override ulong DecodeValue(ULIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        _codec.Decode(buffer);

    protected override byte[] EncodeValue(ULIntDataPoint dataPoint, ulong value) =>
        _codec.Encode(value);
}
