namespace ViciOne.Suite.DataPort.AllenBradley.Legacy.Client.TypeConversion;

/// <summary>
/// ASCII characters packed two per word, with the two characters of each word swapped, as a string
/// (<c>ST</c>) and an ASCII (<c>A</c>) element hold them. libplctag swaps them only in its string
/// accessors, and the raw buffer a converter gets is unswapped.
/// </summary>
internal static class SwappedCharacterPairs
{
    /// <summary>Reads the first <paramref name="length"/> characters from <paramref name="bytes"/>.</summary>
    public static string Decode(ReadOnlySpan<byte> bytes, int length)
    {
        var characters = new char[length];
        for (var index = 0; index < length; index++)
        {
            characters[index] = (char)bytes[OffsetOfCharacter(index)];
        }

        return new string(characters);
    }

    /// <summary>Writes the characters of an ASCII <paramref name="value"/> into <paramref name="bytes"/>.</summary>
    public static void Encode(string value, Span<byte> bytes)
    {
        for (var index = 0; index < value.Length; index++)
        {
            bytes[OffsetOfCharacter(index)] = (byte)value[index];
        }
    }

    // The first character of a word is in its high byte, which comes second on the wire.
    private static int OffsetOfCharacter(int index) => index ^ 1;
}
