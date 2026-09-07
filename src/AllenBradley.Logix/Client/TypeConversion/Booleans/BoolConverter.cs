using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Booleans;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Booleans;

// BOOL (0xC1): one byte, and the only atomic whose decode is not a BinaryPrimitives read. The
// controller stores 0 or 0xFF, but a member set through a mask can leave any nonzero pattern behind, so
// the rule is nonzero rather than equality with 0xFF. Writing picks 0xFF, which is what Studio 5000
// shows for a set BOOL.
internal sealed class BoolConverter : AtomicDataPointConverter<BoolDataPoint, bool>
{
    private const byte True = 0xFF;
    private const byte False = 0x00;

    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.Bool;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.Bool;

    protected override bool DecodeValue(BoolDataPoint dataPoint, ReadOnlySpan<byte> buffer) => buffer[0] != 0;

    protected override byte[] EncodeValue(BoolDataPoint dataPoint, bool value) => [value ? True : False];
}
