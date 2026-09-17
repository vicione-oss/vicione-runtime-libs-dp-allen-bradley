using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Booleans;

/// <summary>
/// BOOL (0xC1): one byte. Any nonzero pattern reads as true, because a member set through a mask can leave
/// one behind; a write picks 0xFF, which is what Studio 5000 shows for a set BOOL.
/// </summary>
internal sealed class BoolConverter : DataPointConverter<BoolDataPoint, bool>
{
    private const byte True = 0xFF;
    private const byte False = 0x00;

    protected override bool DecodeValue(BoolDataPoint dataPoint, ReadOnlySpan<byte> buffer) => buffer[0] != 0;

    protected override byte[] EncodeValue(BoolDataPoint dataPoint, bool value) => [value ? True : False];
}
