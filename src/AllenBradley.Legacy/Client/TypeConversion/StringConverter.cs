using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>
/// Converts an element of a string (<c>ST</c>) file to a <see cref="string"/> and back: a length word, then
/// 82 characters with the two characters of each word swapped. libplctag swaps them only in its string
/// accessors, and the raw buffer this converter gets is unswapped.
/// </summary>
internal sealed class StringConverter : LegacyDataPointConverter<StringDataPoint, string>
{
    private const int CharactersOffset = sizeof(short);
    private const int ElementSize = CharactersOffset + StringDataPoint.MaxLength;

    private readonly Int16Codec _lengthCodec = new();

    protected override string DecodeValue(StringDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < ElementSize)
        {
            throw new DecodeException(
                $"{dataPoint.Address} answered with {buffer.Length} bytes; a string element has {ElementSize}.");
        }

        // The length word is the controller's claim, clamped so that a corrupt one cannot kill a poll.
        var length = Math.Clamp((int)_lengthCodec.Decode(buffer), 0, StringDataPoint.MaxLength);

        var characters = new char[length];
        for (var index = 0; index < length; index++)
        {
            characters[index] = (char)buffer[OffsetOfCharacter(index)];
        }

        return new string(characters);
    }

    protected override byte[] EncodeValue(StringDataPoint dataPoint, string value)
    {
        if (value.Length > StringDataPoint.MaxLength || !value.All(char.IsAscii))
        {
            throw new InvalidOperationException(
                $"Cannot write \"{value}\" to {dataPoint.Address}; a string element holds up to " +
                $"{StringDataPoint.MaxLength} ASCII characters.");
        }

        // Always the whole element: SetBuffer resizes the handle to the array, and a short value must not
        // leave the end of the old one in the controller.
        var bytes = new byte[ElementSize];
        _lengthCodec.Encode((short)value.Length, bytes);
        for (var index = 0; index < value.Length; index++)
        {
            bytes[OffsetOfCharacter(index)] = (byte)value[index];
        }

        return bytes;
    }

    // The first character of a word is in its high byte, which comes second on the wire.
    private static int OffsetOfCharacter(int index) => CharactersOffset + (index ^ 1);
}
