namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// How many characters a Logix string tag can hold — the <c>n</c> of its <c>.DATA : SINT[n]</c> member.
/// </summary>
/// <param name="Value">The character capacity.</param>
public readonly record struct StringMaxLength(int Value)
{
    /// <summary>
    /// The bytes <c>.LEN</c> occupies ahead of <c>.DATA</c>: it is a <c>DINT</c>, not the 16-bit count of
    /// the elementary CIP <c>STRING</c>.
    /// </summary>
    internal const int LengthPrefixBytes = sizeof(int);

    /// <summary>The built-in Logix <c>STRING</c>: <c>.DATA : SINT[82]</c>.</summary>
    public static StringMaxLength Standard => new(82);

    /// <summary>
    /// The capacity a string structure of <paramref name="structureBytes"/> member bytes holds: the
    /// <c>86</c> the <c>@tags</c> listing reports for a built-in <c>STRING</c> is <c>82</c> characters
    /// behind a 4-byte <c>.LEN</c>.
    /// </summary>
    /// <param name="structureBytes">The size of the structure's members, unpadded.</param>
    internal static StringMaxLength OfStructure(int structureBytes) =>
        new(Math.Max(0, structureBytes - LengthPrefixBytes));
}
