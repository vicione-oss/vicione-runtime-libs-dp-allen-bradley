using System.Text;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;

/// <summary>
/// The Logix STRING: a predefined structure, not an elementary type. It is a scalar in the model: one value,
/// not an array. The size of the structure is a multiple of 4 bytes.
/// <para>
/// Layout for the value <c>"Hi"</c> on a built-in string (n = 82). The size is 4 + 82 = 86 bytes, rounded up to 88:
/// </para>
/// <code>
/// Offset  Size  Part                 Content
///      0     4  .LEN                 02 00 00 00
///      4     2  .DATA, characters    48 69
///      6    80  .DATA, rest          00 ... 00
///     86     2  Padding              00 00
/// </code>
/// </summary>
internal sealed class LogixStringConverter : DataPointConverter<StringDataPoint, string>
{
    private static readonly int DataOffset = LogixStringHeader.Size;
    private static readonly Encoding Latin1 = Encoding.Latin1;

    protected override string DecodeValue(StringDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var claimedLength = LogixStringHeader.ReadFrom(buffer).Length;

        // .LEN is the controller's unchecked claim about its own .DATA, and a signed DINT at that: clamped
        // rather than rejected, so an over-long or negative one cannot read past the reply or kill a poll.
        var capacity = Math.Min(dataPoint.MaxLength.Value, buffer.Length - DataOffset);
        var characterCount = Math.Clamp(claimedLength, 0, capacity);

        return Latin1.GetString(buffer.Slice(DataOffset, characterCount));
    }

    protected override byte[] EncodeValue(StringDataPoint dataPoint, string value)
    {
        var characterCount = Latin1.GetByteCount(value);
        if (characterCount > dataPoint.MaxLength.Value)
        {
            throw new InvalidOperationException(
                $"Cannot write {characterCount} characters to {dataPoint.TagAddress}; " +
                $"the tag is configured to hold {dataPoint.MaxLength.Value}.");
        }

        var bytes = CreateByteBuffer(dataPoint);
        new LogixStringHeader(characterCount).WriteTo(bytes);
        Latin1.GetBytes(value, bytes.AsSpan(DataOffset));
        return bytes;
    }

    // The buffer covers the whole of .DATA, not only the characters in hand. The controller keeps what is
    // written past .LEN, so the zeroed tail stops a shorter value from leaving the old one visible in Studio 5000.
    private static byte[] CreateByteBuffer(StringDataPoint dataPoint)
    {
        var sizeWithoutPadding = DataOffset + dataPoint.MaxLength.Value;
        return new byte[RoundUpToMultipleOfFour(sizeWithoutPadding)];
    }

    // A 5X80 refuses a write that is shorter than the whole structure, and the structure ends on a 4-byte
    // boundary. Example: 4 (.LEN) + 82 (.DATA) = 86 bytes, rounded up to 88.
    private static int RoundUpToMultipleOfFour(int size) => (size + 3) / 4 * 4;
}
