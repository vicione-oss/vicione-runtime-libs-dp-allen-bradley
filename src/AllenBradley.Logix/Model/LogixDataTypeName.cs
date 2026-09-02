namespace ViciOne.Suite.DataPort.AllenBradley.Logix.Model;

/// <summary>
/// The Logix spelling of a data type — <c>DINT</c>, <c>REAL</c>, <c>STRING</c> — as Studio 5000 names it.
/// </summary>
/// <remarks>
/// <see cref="AllenBradleyDataType"/> names the elementary types, which is what a comparison needs but
/// covers only those: a structured type such as <c>STRING</c> has no member to name it by at all. This
/// is the name a converter reports for messages; it is display, never identity.
/// </remarks>
/// <param name="Value">The type name.</param>
public readonly record struct LogixDataTypeName(string Value)
{
    /// <summary>A 32-bit signed integer.</summary>
    public static LogixDataTypeName DInt => new("DINT");

    /// <summary>An IEEE-754 single.</summary>
    public static LogixDataTypeName Real => new("REAL");

    /// <summary>An IEEE-754 double. A 5X80 controller's type; a 5X70 has no such thing.</summary>
    public static LogixDataTypeName LReal => new("LREAL");

    /// <summary>The predefined string structure — <c>.LEN</c> + <c>.DATA</c>.</summary>
    public static LogixDataTypeName String => new("STRING");

    /// <summary>The type name, so it interpolates and logs as itself.</summary>
    public override string ToString() => Value;
}
