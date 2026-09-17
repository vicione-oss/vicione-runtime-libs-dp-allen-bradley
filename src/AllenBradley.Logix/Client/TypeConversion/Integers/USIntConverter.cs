using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Integers;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Integers;

/// <summary>
/// USINT (0xC6): 8-bit unsigned integer — the same byte as a SINT, told apart only by the declared type,
/// since nothing on the wire records what the top bit means.
/// </summary>
internal sealed class USIntConverter : DataPointConverter<USIntDataPoint, byte>
{
    internal const int ElementSize = sizeof(byte);

    internal static byte DecodeElement(ReadOnlySpan<byte> buffer) => buffer[0];

    internal static byte[] EncodeElement(byte value) => [value];

    protected override byte DecodeValue(USIntDataPoint dataPoint, ReadOnlySpan<byte> buffer) =>
        DecodeElement(buffer);

    protected override byte[] EncodeValue(USIntDataPoint dataPoint, byte value) =>
        EncodeElement(value);
}
