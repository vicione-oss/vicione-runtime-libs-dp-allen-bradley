using ViciOne.Suite.DataPort.AllenBradley.Client.Shared.TypeConversion;
using ViciOne.Suite.DataPort.AllenBradley.Legacy.Model.DataPort.DataPoints.Strings;

namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>
/// Converts an element of an ASCII (<c>A</c>) file to a <see cref="string"/> and back: one word, two
/// characters in <see cref="SwappedCharacterPairs"/>. The element has no length word, so a trailing NUL
/// character ends the value.
/// </summary>
internal sealed class AsciiConverter : LegacyDataPointConverter<AsciiDataPoint, string>
{
    private const int ElementSize = AsciiDataPoint.MaxLength;

    protected override string DecodeValue(AsciiDataPoint dataPoint, ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < ElementSize)
        {
            throw new DecodeException(
                $"{dataPoint.Address} answered with {buffer.Length} bytes; an ASCII element has {ElementSize}.");
        }

        return SwappedCharacterPairs.Decode(buffer, AsciiDataPoint.MaxLength).TrimEnd('\0');
    }

    protected override byte[] EncodeValue(AsciiDataPoint dataPoint, string value)
    {
        if (value.Length > AsciiDataPoint.MaxLength || !value.All(char.IsAscii))
        {
            throw new InvalidOperationException(
                $"Cannot write \"{value}\" to {dataPoint.Address}; an ASCII element holds up to " +
                $"{AsciiDataPoint.MaxLength} ASCII characters.");
        }

        var bytes = new byte[ElementSize];
        SwappedCharacterPairs.Encode(value, bytes);

        return bytes;
    }
}
