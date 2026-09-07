using System.Text;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.Scalars.Strings;
using ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Client.TypeConversion.Strings;

// The Logix STRING: a predefined structure, not an elementary type — a LogixStringHeader followed by
// .DATA : SINT[n], the whole padded up to a 4-byte boundary (88 bytes for the built-in n = 82). It is a
// scalar in the model all the same: one value, not an array.
internal sealed class LogixStringConverter : DataPointConverter<StringDataPoint, string>
{
    // Where .DATA begins. Not a const, because a marshalled size is not a constant expression.
    private static readonly int DataOffset = LogixStringHeader.Size;

    // Latin-1, matching the sibling S7 addon's StringAccessBufferSetter, so a value written through one
    // port reads back the same through the other. Characters outside Latin-1 encode as '?' — that is the
    // replacement fallback, and it is lossy by design: a STRING stores one byte per character and there
    // is nothing else to store.
    private static readonly Encoding Latin1 = Encoding.Latin1;

    public override LogixDataTypeName ExpectedTypeName => LogixDataTypeName.String;

    public override LogixTypeKind ExpectedKind => LogixTypeKind.Structure;

    public override AllenBradleyDataType? ExpectedDataType => AllenBradleyDataType.String;

    // The one converter whose expectation is not a constant: STRING and STRING_20 both route here, and
    // the capacity the controller must agree with is the data point's own configuration.
    protected override StringMaxLength? MaxLengthOf(StringDataPoint dataPoint) => dataPoint.MaxLength;

    protected override string DecodeValue(StringDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        var claimedLength = LogixStringHeader.ReadFrom(buffer).Length;

        // .LEN is where the value ends — whatever sits past it in .DATA is the tail of some longer value
        // written earlier, and no Logix client reads it. The claim itself is the controller's, though, and
        // nothing has checked it: one past the end of .DATA would read the alignment padding as text, one
        // past the end of a short reply would read off the buffer, and a negative one is reachable because
        // .LEN is a signed DINT. Clamped rather than rejected — a poll must not die on it.
        var capacity = Math.Min(dataPoint.MaxLength.Value, buffer.Length - DataOffset);
        var characterCount = Math.Clamp(claimedLength, 0, capacity);

        return Latin1.GetString(buffer.Slice(DataOffset, characterCount));
    }

    protected override byte[] EncodeValue(StringDataPoint dataPoint, string value)
    {
        var characterCount = Latin1.GetByteCount(value);
        if (characterCount > dataPoint.MaxLength.Value)
        {
            // Truncating here would write a value the caller never asked for and report success for it.
            throw new InvalidOperationException(
                $"Cannot write {characterCount} characters to {dataPoint.TagName}; " +
                $"the tag is configured to hold {dataPoint.MaxLength.Value}.");
        }

        // .LEN and the whole of .DATA, not just the characters in hand. The controller keeps whatever
        // is written past .LEN, so a shorter value over a longer one would otherwise leave the old tail
        // sitting in the tag — invisible to a reader that honours .LEN, and very visible to anyone
        // looking at the tag in Studio 5000. A fresh array is zero past the characters, and every byte
        // of it reaches the handle. The alignment padding after .DATA is not here: the controller
        // decides that, and libplctag's handle carries it.
        var bytes = new byte[DataOffset + dataPoint.MaxLength.Value];
        new LogixStringHeader(characterCount).WriteTo(bytes);
        Latin1.GetBytes(value, bytes.AsSpan(DataOffset));
        return bytes;
    }
}
