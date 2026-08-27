using System.Buffers.Binary;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion;

// DINT (0xC4): 32-bit little-endian signed integer. CIP and .NET are both little-endian, so this is a
// direct read/write with no byte swap.
internal sealed class DIntConverter : DataPointConverter<DIntDataPoint, int>
{
    public override CipType ExpectedType => CipType.Dint;

    public override ByteSize ByteSize => new(sizeof(int));

    protected override int DecodeValue(DIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        BinaryPrimitives.ReadInt32LittleEndian(buffer);

    protected override void EncodeValue(DIntDataPoint dataPoint, int value, Span<byte> buffer) =>
        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
}
