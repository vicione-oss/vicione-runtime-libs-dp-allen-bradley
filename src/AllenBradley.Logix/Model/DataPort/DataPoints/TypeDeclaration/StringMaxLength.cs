namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model.DataPort.DataPoints.TypeDeclaration;

/// <summary>
/// How many characters a Logix string tag can hold — the <c>n</c> of its <c>.DATA : SINT[n]</c> member.
/// </summary>
/// <param name="Value">The character capacity.</param>
public readonly record struct StringMaxLength(int Value)
{
    /// <summary>The built-in Logix <c>STRING</c>: <c>.DATA : SINT[82]</c>.</summary>
    public static StringMaxLength Standard => new(82);
}
