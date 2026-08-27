namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// How many characters a Logix string tag can hold — the <c>n</c> of its <c>.DATA : SINT[n]</c> member.
/// </summary>
/// <remarks>
/// It is what the controller's own listing reports, which <see cref="TagDefinition"/> carries, and what
/// a tag is declared as in Studio 5000. The built-in <c>STRING</c> is <see cref="Standard"/>; a custom
/// string type (<c>STRING_20</c>, <c>STRING_100</c>) has the same <c>.LEN</c> + <c>.DATA[n]</c> shape
/// and a different <c>n</c>.
/// </remarks>
/// <param name="Value">The character capacity.</param>
public readonly record struct StringMaxLength(int Value)
{
    /// <summary>
    /// The bytes <c>.LEN</c> occupies ahead of <c>.DATA</c>: it is a DINT, not the 16-bit count of the
    /// elementary CIP <c>STRING</c>. A string structure is that much wider than its capacity, which is
    /// the one arithmetic fact that turns a declared structure size into a capacity and back.
    /// </summary>
    internal const int LengthPrefixBytes = sizeof(int);

    /// <summary>The built-in Logix <c>STRING</c>: <c>.DATA : SINT[82]</c>.</summary>
    public static StringMaxLength Standard => new(82);

    /// <summary>
    /// The capacity a string structure of <paramref name="structureBytes"/> member bytes holds — the
    /// <c>86</c> the <c>@tags</c> listing reports for a built-in <c>STRING</c> is <c>82</c> characters
    /// behind a 4-byte <c>.LEN</c>. Clamped at zero so a structure too small to be a string decodes as
    /// a capacity of none rather than a negative one.
    /// </summary>
    /// <param name="structureBytes">The size of the structure's members, unpadded.</param>
    internal static StringMaxLength OfStructure(int structureBytes) =>
        new(Math.Max(0, structureBytes - LengthPrefixBytes));
}
