using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion.Codecs;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>
/// Converts an element of a string (<c>ST</c>) file to a <see cref="string"/> and back: a length word, then
/// 82 characters in <see cref="SwappedCharacterPairs"/>.
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

        return SwappedCharacterPairs.Decode(buffer[CharactersOffset..], length);
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
        SwappedCharacterPairs.Encode(value, bytes.AsSpan(CharactersOffset));

        return bytes;
    }
}
