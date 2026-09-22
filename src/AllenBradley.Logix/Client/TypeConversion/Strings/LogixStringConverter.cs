using System.Text;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;

/// <summary>
/// The Logix STRING: a predefined structure, not an elementary type — a <see cref="LogixStringHeader"/>
/// followed by <c>.DATA : SINT[n]</c>, the whole padded up to a 4-byte boundary (88 bytes for the built-in
/// n = 82). A scalar in the model all the same: one value, not an array.
/// </summary>
internal sealed class LogixStringConverter : DataPointConverter<StringDataPoint, string>
{
    // Not a const: a marshalled size is not a constant expression.
    private static readonly int DataOffset = LogixStringHeader.Size;

    // Matches the sibling S7 dataport's StringAccessBufferSetter, so a value written through one port reads
    // back the same through the other. One byte per character, so anything outside Latin-1 encodes as '?'.
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

        // The whole of .DATA, not just the characters in hand: the controller keeps what is written past
        // .LEN, so the zeroed tail is what stops a shorter value leaving the old one visible in Studio 5000.
        var bytes = new byte[DataOffset + dataPoint.MaxLength.Value];
        new LogixStringHeader(characterCount).WriteTo(bytes);
        Latin1.GetBytes(value, bytes.AsSpan(DataOffset));
        return bytes;
    }
}
